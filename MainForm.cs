using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace YobaLoncher {
	public partial class MainForm : Form {
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		private DownloadProgressTracker downloadProgressTracker_;
		public string ThePath = "";
		private LinkedList<FileInfo> filesToUpload_;
		private LinkedList<FileInfo> modFilesToUpload_;
		private LinkedListNode<FileInfo> currentFile_ = null;
		private bool ReadyToGo_ = false;
		private bool LaunchButtonEnabled_ = false;
		private volatile bool UpdateInProgress_ = false;

		private long lastDlstringUpdate_ = -1;

		private ProgressBarInfo progressBarInfo_ = new ProgressBarInfo();

		WebClient wc_;

		internal class WebModInfo {
			public bool DlInProgress = false;
			public bool Installed = false;
			public bool Active = false;
			public string Name;
			public string Description;
			[JsonIgnore]
			public ModInfo ModInfo;

			public WebModInfo(ModInfo mi) {
				DlInProgress = mi.DlInProgress;
				Description = mi.CurrentVersionData.Description ?? mi.Description;
				Name = mi.CurrentVersionData.Name ?? mi.Name;
				ModInfo = mi;
				if (mi.ModConfigurationInfo != null) {
					Installed = true;
					Active = mi.ModConfigurationInfo.Active;
				}
			}
		}
		internal class ProgressBarInfo {
			public int MaxValue = 1000;
			public int Progress = 0;
			public string Caption = "";
		}

		public MainForm() {
			ThePath = Program.GamePath;

			InitializeComponent();

			//int winstyle = NativeWinAPI.GetWindowLong(basePanel.Handle, NativeWinAPI.GWL_EXSTYLE);
			//NativeWinAPI.SetWindowLong(basePanel.Handle, NativeWinAPI.GWL_EXSTYLE, winstyle | NativeWinAPI.WS_EX_COMPOSITED);

			SuspendLayout();
			
			wc_ = new WebClient { Encoding = Encoding.UTF8 };
			wc_.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChanged);
			wc_.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadCompleted);
			downloadProgressTracker_ = new DownloadProgressTracker(50, TimeSpan.FromMilliseconds(500));

			int missingFilesCount = Program.GameFileCheckResult.InvalidFiles.Count;

			if (missingFilesCount > 0) {
				filesToUpload_ = Program.GameFileCheckResult.InvalidFiles;
				bool allowRun = true;
				foreach (FileInfo fi in filesToUpload_) {
					if (fi.Importance < 2 && (!fi.IsPresent || (!fi.IsOK && fi.Importance < 1))) {
						allowRun = false;
					}
					else {
						missingFilesCount--;
					}
				}
				SetReady(allowRun);
			}
			else {
				SetReady(true);
			}
			progressBarInfo_.Caption = ReadyToGo_ ? Locale.Get("AllFilesIntact") : String.Format(Locale.Get("FilesMissing"), missingFilesCount);

			Text = Locale.Get("MainFormTitle");

			mainBrowser.Size = new Size(780, 440);
			mainBrowser.Location = new Point(0, 0);
			mainBrowser.ObjectForScripting = YobaWebController.Instance;
			YobaWebController.Instance.Form = this;
			mainBrowser.Navigated += MainBrowser_Navigated;
			mainBrowser.Navigating += webBrowser_Navigating;
			UpdateMainWebView();

			/*for (int i = 0; i < Program.LoncherSettings.Buttons.Count; i++) {
				LinkButton lbtn = Program.LoncherSettings.Buttons[i];
				if (lbtn != null) {
					YobaButton linkButton = new YobaButton(lbtn.Url);
					linkButton.Name = "linkBtn" + (i + 1);
					linkButton.TabIndex = 10 + i;
					linkButton.UseVisualStyleBackColor = true;
					linkButton.ApplyUIStyles(lbtn);
					if (YU.stringHasText(lbtn.Caption)) {
						linkButton.Text = "";
						theToolTip.SetToolTip(linkButton, lbtn.Caption);
					}
					linkButton.Click += new EventHandler((object o, EventArgs a) => {
						string url = ((YobaButton)o).Url;
						if (YU.stringHasText(url)) {
							Process.Start(url);
						}
					});
					linksPanel.Controls.Add(linkButton);
				}
			}*/

			BackgroundImageLayout = ImageLayout.Stretch;
			BackgroundImage = Program.LoncherSettings.Background;

			List<ModInfo> outdatedMods = new List<ModInfo>();
			LinkedList<FileInfo> outdatedModFiles = new LinkedList<FileInfo>();
			LinkedList<FileInfo> outdatedAlteredModFiles = new LinkedList<FileInfo>();

			foreach (ModInfo mi in Program.LoncherSettings.Mods) {
				if (mi.CurrentVersionFiles != null) {
					if ((mi.ModConfigurationInfo != null) && mi.ModConfigurationInfo.Active) {
						bool hasit = false;
						foreach (FileInfo mif in mi.CurrentVersionFiles) {
							if (!mif.IsOK) {
								outdatedModFiles.AddLast(mif);
								if (!hasit) {
									outdatedMods.Add(mi);
									hasit = true;
								}
								if (mi.ModConfigurationInfo.Altered) {
									outdatedAlteredModFiles.AddLast(mif);
								}
							}
						}
						if (hasit) {
							outdatedModFiles.Last.Value.LastFileOfModToUpdate = mi;
						}
					}
				}
			}
			if (outdatedMods.Count > 0) {
				string outdatedmods = "";
				string alteredmods = "";
				List<ModInfo> alteredOutdatedMods = new List<ModInfo>();
				ulong outdatedmodssize = 0;
				bool comma = false;
				bool altcomma = false;
				foreach (ModInfo mi in outdatedMods) {
					if (!comma) {
						comma = true;
					}
					else {
						outdatedmods += ", ";
					}
					outdatedmods += mi.CurrentVersionData.Name ?? mi.Name;
					if (mi.ModConfigurationInfo.Altered) {
						if (!altcomma) {
							altcomma = true;
						}
						else {
							alteredmods += ", ";
						}
						alteredOutdatedMods.Add(mi);
						alteredmods += mi.CurrentVersionData.Name ?? mi.Name;
					}
				}
				foreach (FileInfo mif in outdatedModFiles) {
					outdatedmodssize += mif.Size;
				}
				if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("YouHaveOutdatedMods"), outdatedmods, YU.formatFileSize(outdatedmodssize)), YobaDialog.YesNoBtns)) {
					modFilesToUpload_ = outdatedModFiles;
					foreach (ModInfo mi in outdatedMods) {
						mi.DlInProgress = true;
					}
					UpdateModsWebView();
					if (!UpdateInProgress_) {
						DownloadNextMod();
					}
				}
				else {
					if (alteredOutdatedMods.Count > 0) {
						ulong alteredmodssize = 0;
						foreach (FileInfo mif in outdatedAlteredModFiles) {
							alteredmodssize += mif.Size;
						}
						if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("YouHaveAlteredMods"), alteredmods, YU.formatFileSize(alteredmodssize)), YobaDialog.YesNoBtns)) {
							modFilesToUpload_ = outdatedAlteredModFiles;
							foreach (ModInfo mi in alteredOutdatedMods) {
								mi.DlInProgress = true;
							}
							UpdateModsWebView();
							if (!UpdateInProgress_) {
								DownloadNextMod();
							}
						}
					}
				}
			}
			PerformLayout();
		}

		private void MainBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
			mainBrowser.Visible = true;
			minimizeButton.Visible = false;
			closeButton.Visible = false;
			helpButton.Visible = false;
#if DEBUG
			refreshButton.Visible = true;
#endif
		}

		private void UpdateStatusWebView() {
			GameVersion gameVersion = Program.LoncherSettings.GameVersion;
			gameVersion.ResetCheckedToDl();
			string gvstr = JsonConvert.SerializeObject(gameVersion);
			string result = new StringBuilder("{\"GameVersion\":", gvstr.Length + 100)
				.Append(gvstr)
				.Append(",\"LaunchBtn\":")
				.Append(GetLaunchButtonStateJson())
				.Append("}").ToString();

			RunScript("__updateStatusView", new object[] { result });
			return;
		}

		private void UpdateLaunchButton() {
			RunScript("__updateLaunchBtn", new object[] { GetLaunchButtonStateJson() });
			return;
		}

		private string GetLaunchButtonStateJson() {
			return "{\"IsReady\":" + (ReadyToGo_ ? "true" : "false")
				+ ",\"Enabled\":" + (LaunchButtonEnabled_ ? "true" : "false") + "}";
		}

		private void UpdateModsWebView() {
			string modstr = JsonConvert.SerializeObject(YobaWebController.Instance.ModList);
			RunScript("__updateModsView", new object[] { modstr });
		}

		private void RunScript(string funcName, object[] args) {
			if (mainBrowser.Visible) {
				mainBrowser.Document.InvokeScript(funcName, args);
			}
		}
		private void RunScript(string funcName) {
			if (mainBrowser.Visible) {
				mainBrowser.Document.InvokeScript(funcName);
			}
		}
		private void UpdateProgressBar(int progress) {
			progressBarInfo_.Progress = progress;
			RunScript("__updateProgressBar", new object[] { progress });
		}
		private void UpdateProgressBar(int progress, string labelText) {
			progressBarInfo_.Progress = progress;
			progressBarInfo_.Caption = labelText;
			RunScript("__updateProgressBar", new object[] { progress, labelText });
		}

		private void UpdateMainWebView() {
			LauncherData ls = Program.LoncherSettings;

			if (ls.MainPage.Error != null) {
				YU.ErrorAndKill(Locale.Get(ls.MainPage.Error));
			}
			else {
				mainBrowser.Navigate(new Uri(ls.MainPage.Site));
			}
		}

		private async void DownloadFile(FileInfo fileInfo) {
			if (!YU.stringHasText(fileInfo.UploadAlias)) {
				fileInfo.UploadAlias = fileInfo.Hashes.Count > 0 ? fileInfo.Hashes[0] : null;
				if (!YU.stringHasText(fileInfo.UploadAlias)) {
					int lios = Math.Max(fileInfo.Path.LastIndexOf('\\'), fileInfo.Path.LastIndexOf('/'));
					fileInfo.UploadAlias = lios > -1 ? fileInfo.Path.Substring(lios + 1, fileInfo.Path.Length) : fileInfo.Path;
				}
			}
			string uploadFilename = PreloaderForm.UPDPATH + fileInfo.UploadAlias;
			if (File.Exists(uploadFilename)) {
				if (FileChecker.CheckFileMD5(PreloaderForm.UPDPATH, fileInfo)) {
					if (UpdateInProgress_) {
						DownloadNext();
					}
					else {
						DownloadNextMod();
					}
					return;
				}
				else {
					File.Delete(uploadFilename);
				}
			}
			try {
				string labelText = string.Format(
					Locale.Get("DLRate")
					, YU.formatFileSize(0)
					, YU.formatFileSize(fileInfo.Size)
					, ""
					, currentFile_.Value.Description
				);
				UpdateProgressBar(0, labelText);
				await wc_.DownloadFileTaskAsync(new Uri(fileInfo.Url), uploadFilename);
			}
			catch (Exception ex) {
				ShowDownloadError(string.Format(Locale.Get("CannotDownloadFile"), fileInfo.Path) + "\r\n" + ex.Message);
			}
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			downloadProgressTracker_.SetProgress(e.BytesReceived, e.TotalBytesToReceive);
			if (DateTime.Now.Ticks > lastDlstringUpdate_ + 500000L) {
				lastDlstringUpdate_ = DateTime.Now.Ticks;
				int progressVal = (int)Math.Floor((double)e.BytesReceived / e.TotalBytesToReceive * progressBarInfo_.MaxValue);
				string labelText = string.Format(
					Locale.Get("DLRate")
					, YU.formatFileSize(e.BytesReceived)
					, YU.formatFileSize(e.TotalBytesToReceive)
					, downloadProgressTracker_.GetBytesPerSecondString()
					, ((currentFile_ is null) ? "" : currentFile_.Value.Description)
				);
				UpdateProgressBar(progressVal, labelText);
			}
		}

		private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e) {
			downloadProgressTracker_.Reset();
			if (UpdateInProgress_) {
				DownloadNext();
			}
			else {
				DownloadNextMod();
			}
		}

		private void DownloadNext() {
			do {
				currentFile_ = currentFile_.Next;
			}
			while ((currentFile_ != null) && !currentFile_.Value.IsCheckedToDl);
			
			if (currentFile_ != null) {
				DownloadFile(currentFile_.Value);
			}
			else {
				string filename = "";
				UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("StatusCopyingFiles"));
				try {
					foreach (FileInfo fileInfo in filesToUpload_) {
						if (!fileInfo.IsCheckedToDl) {
							continue;
						}
						filename = ThePath + fileInfo.Path.Replace('/', '\\');
						string dirpath = filename.Substring(0, filename.LastIndexOf('\\'));
						Directory.CreateDirectory(dirpath);
						if (File.Exists(filename)) {
							File.Delete(filename);
						}
						File.Move(PreloaderForm.UPDPATH + fileInfo.UploadAlias, filename);
						fileInfo.IsOK = true;
						fileInfo.IsPresent = true;
					}

					UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("StatusUpdatingDone"));
					UpdateInProgress_ = false;
					if (modFilesToUpload_ != null) {
						UpdateStatusWebView();
						DownloadNextMod();
					}
					else {
						LaunchButtonEnabled_ = true;
						SetReady(true);
						if (YobaDialog.ShowDialog(Locale.Get("UpdateSuccessful"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
							launch();
						}
					}
				}
				catch (UnauthorizedAccessException ex) {
					ShowDownloadError(string.Format(Locale.Get("DirectoryAccessDenied"), filename) + ":\r\n" + ex.Message);
				}
				catch (Exception ex) {
					ShowDownloadError(string.Format(Locale.Get("CannotMoveFile"), filename) + ":\r\n" + ex.Message);
				}
				UpdateInProgress_ = false;
			}
		}

		private void ShowDownloadError(string error) {
			YobaDialog.ShowDialog(error);
			LaunchButtonEnabled_ = true;
			UpdateProgressBar(0, Locale.Get("StatusDownloadError"));
			UpdateStatusWebView();
		}

		private void SetReady(bool isReady) {
			ReadyToGo_ = isReady;
			LaunchButtonEnabled_ = true;
			UpdateStatusWebView();
		}
		private void CheckReady() {
			if (filesToUpload_ != null) {
				foreach (FileInfo fi in filesToUpload_) {
					if (fi.IsCheckedToDl) {
						return;
					}
				}
			}
			SetReady(true);
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
			Application.Exit();
		}

		public void OnLaunchGameBtn() {
			if (ReadyToGo_) {
				launch();
			}
			else {
				LaunchButtonEnabled_ = false;
				UpdateInProgress_ = true;
				UpdateStatusWebView();
				currentFile_ = filesToUpload_.First;
				while ((currentFile_ != null) && !currentFile_.Value.IsCheckedToDl) {
					currentFile_ = currentFile_.Next;
				}
				DownloadFile(currentFile_.Value);
			}
		}

		private void launchGameBtn_Click(object sender, EventArgs e) {
			OnLaunchGameBtn();
		}

		private void launch() {
			string args = "/C \"" + ThePath + Program.LoncherSettings.ExeName + "\"";
			if (LauncherConfig.LaunchFromGalaxy) {
				args = string.Format("/command=runGame /gameId={1} /path=\"{0}\"", ThePath, Program.LoncherSettings.GogID);
				Process.Start(new ProcessStartInfo { Arguments = args, FileName = LauncherConfig.GalaxyDir });
			}
			else {
				if (ThePath.Contains("steamapps")) {
					args = "/C explorer steam://run/" + Program.LoncherSettings.SteamID;
				}
				Process.Start(new ProcessStartInfo { Arguments = args, FileName = "cmd", WindowStyle = ProcessWindowStyle.Hidden });
			}
			YU.Log(args);
			LaunchButtonEnabled_ = false;
			UpdateLaunchButton();
			System.Threading.Thread.Sleep(1800);
			if (LauncherConfig.CloseOnLaunch) {
				Application.Exit();
			}
			else {
				LaunchButtonEnabled_ = true;
				UpdateLaunchButton();
			}
		}

		private void settingsButton_Click(object sender, EventArgs e) {
			ShowSettingsDialog();
		}

		public void ShowSettingsDialog() {
			SettingsDialog settingsDialog = new SettingsDialog(this);
			settingsDialog.Icon = Program.LoncherSettings.Icon;
			if (settingsDialog.ShowDialog(this) == DialogResult.OK) {
				LauncherConfig.StartPage = settingsDialog.OpeningPanel;
				LauncherConfig.GameDir = settingsDialog.GamePath;
				LauncherConfig.LaunchFromGalaxy = settingsDialog.LaunchViaGalaxy;
				bool prevOffline = LauncherConfig.StartOffline;
				LauncherConfig.StartOffline = settingsDialog.OfflineMode;
				LauncherConfig.CloseOnLaunch = settingsDialog.CloseOnLaunch;
				LauncherConfig.Save();
				settingsDialog.Dispose();
				if (LauncherConfig.GameDir != Program.GamePath) {
					Hide();
					new PreloaderForm(this).Show();
				}
				else if (prevOffline != LauncherConfig.StartOffline) {
					if (YobaDialog.ShowDialog(Locale.Get(LauncherConfig.StartOffline ? "OfflineModeSet" : "OnlineModeSet"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
						Hide();
						new PreloaderForm(this).Show();
					}
				}
			}
			this.Focus();
		}

		private void closeButton_Click(object sender, EventArgs e) {
			Application.Exit();
		}

		private void minimizeButton_Click(object sender, EventArgs e) {
			WindowState = FormWindowState.Minimized;
		}

		private void draggingPanel_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}

		private async Task<LauncherData.StaticTabData> getStaticTabData(string uiKey, string url, string quoteToEscape, string replacePlaceholder) {
			LauncherData.StaticTabData staticTabData = new LauncherData.StaticTabData();
			staticTabData.Site = url;
			try {
				if (Program.LoncherSettings.UIStyle.TryGetValue(uiKey, out FileInfo fileInfo)) {
					if (fileInfo != null && YU.stringHasText(fileInfo.Url)) {
						using (WebClient wc = new WebClient { Encoding = Encoding.UTF8 }) {
							string template = (await wc.DownloadStringTaskAsync(new Uri(fileInfo.Url)));
							string cl = "";
							if (url != null && url.Length > 0) {
								cl = (await wc.DownloadStringTaskAsync(new Uri(url)));
								if (quoteToEscape != null && quoteToEscape.Length > 0) {
									string quote = quoteToEscape;
									cl = cl.Replace("\\", "\\\\").Replace(quote, "\\" + quote);
									if (cl.Contains("\r")) {
										cl = cl.Replace("\r\n", "\\\r\n");
									}
									else {
										cl = cl.Replace("\n", "\\\n");
									}
								}
							}
							staticTabData.Html = template.Replace(replacePlaceholder, cl);
						}
					}
				}
			}
			catch (Exception ex) {
				staticTabData.Error = ex.Message;
			}
			return staticTabData;
		}

		private async void refreshButton_Click(object sender, EventArgs e) {
			if (!Program.OfflineMode) {
				
			}
		}

		private void helpButton_Click(object sender, EventArgs e) {
			YU.ShowHelpDialog();
		}

		private void MainForm_Shown(object sender, EventArgs e) {
			if (!(Program.FirstRun || Program.OfflineMode)
					&& Program.LoncherSettings.Survey != null
					&& YU.stringHasText(Program.LoncherSettings.Survey.Text)
					&& YU.stringHasText(Program.LoncherSettings.Survey.Url)
					&& YU.stringHasText(Program.LoncherSettings.Survey.ID)
					&& (LauncherConfig.LastSurveyId is null || LauncherConfig.LastSurveyId != Program.LoncherSettings.Survey.ID)) {
				int showSurvey = new Random().Next(0, 100);
				string discardId = "-0" + Program.LoncherSettings.Survey.ID;
				bool wasDiscarded = discardId.Equals(LauncherConfig.LastSurveyId);
				if ((!wasDiscarded && showSurvey > 70) || (showSurvey > 7 && showSurvey < 10)) {
					DialogResult result = YobaDialog.ShowDialog(Program.LoncherSettings.Survey.Text, YobaDialog.YesNoBtns);
					if (result == DialogResult.Yes) {
						Process.Start(Program.LoncherSettings.Survey.Url);
						LauncherConfig.LastSurveyId = Program.LoncherSettings.Survey.ID;
						LauncherConfig.Save();
					}
					else if (result == DialogResult.No) {
						LauncherConfig.LastSurveyId = discardId;
						LauncherConfig.Save();
					}
				}
			}
		}

		private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
			string url = e.Url.ToString();
			string urlstart = url.Substring(0, 7);
			switch (urlstart) {
				case "http://":
				case "https:/":
					Process.Start(url);
					e.Cancel = true;
					break;
			}
		}
	}
}