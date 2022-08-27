using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
using CommonOpenFileDialog = Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog;
using CommonFileDialogResult = Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult;
using System.Diagnostics;

namespace YobaLoncher {
	partial class MainForm {
		[ComVisible(true)]
		public class YobaWebController {
			private static YobaWebController _instance = null;

			internal MainForm Form = null;

			public static YobaWebController Instance {
				get {
					if (_instance is null) {
						_instance = new YobaWebController();
					}
					return _instance;
				}
			}

			public string GetLoc(string key) {
				return Locale.Get(key);
			}
			public string GetLocs(string keysStr) {
				string[] keys = keysStr.Split(',');
				Dictionary<string, string> strings = new Dictionary<string, string>();
				for (int i = 0; i < keys.Length; i++) {
					string key = keys[i].Trim();
					if (!strings.ContainsKey(key)) {
						strings.Add(key, Locale.Get(key));
					}
				}
				return JsonConvert.SerializeObject(strings);
			}
			public void Info(string text) {
				YobaDialog.ShowDialog(text);
			}
			public void Info(string text, string onOk) {
				YobaDialog.ShowDialog(text);
				Form.mainBrowser.Document.InvokeScript(onOk);
			}
			public void Ask(string text, string onYes, string onNo) {
				if (YobaDialog.ShowDialog(text, YobaDialog.YesNoBtns) == DialogResult.Yes) {
					Form.mainBrowser.Document.InvokeScript(onYes);
				}
				else {
					Form.mainBrowser.Document.InvokeScript(onNo);
				}
			}
			public void Warn(string text) {
				MessageBox.Show(text);
			}
			public void Warn(string text, string onOk) {
				MessageBox.Show(text);
				Form.mainBrowser.Document.InvokeScript(onOk);
			}
			public void Warn(string text, string onYes, string onNo) {
				if (MessageBox.Show(Form, text, "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
					Form.mainBrowser.Document.InvokeScript(onYes);
				}
				else {
					Form.mainBrowser.Document.InvokeScript(onNo);
				}
			}
			public void Error(string text) {
				MessageBox.Show(Form, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			public void Error(string text, string onOk) {
				MessageBox.Show(Form, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Form.mainBrowser.Document.InvokeScript(onOk);
			}

			public void Close() {
				Form.ExitApp();
			}
			public void Minimize() {
				Form.WindowState = FormWindowState.Minimized;
			}
			public void Help() {
				YU.ShowHelpDialog();
			}
			public void Settings() {
				Form.ShowSettingsDialog();
			}

			public string RetrieveBackground() {
				return Program.LoncherSettings.BackgroundPath.Substring(12).Replace('\\', '/');
			}
			public string RetrieveStartViewId() {
				return LauncherConfig.StartPage.ToString();
			}
			public void UpdateAppControlsSize(string width, string height) {
				if (Int32.TryParse(width, out int ww)) {
					Form.draggingPanel.WidthSpace = ww;
				}
				if (Int32.TryParse(height, out int hh)) {
					Form.draggingPanel.UpdateSize(Form.Width, hh);
				}
				else {
					Form.draggingPanel.UpdateWidth(Form.Width);
				}
			}
			public int GetProgressBarMax() {
				return Form.progressBarInfo_.MaxValue;
			}
			public string GetProgressBarState() {
				return JsonConvert.SerializeObject(Form.progressBarInfo_);
			}

			public void UpdateStatusWebView() {
				Form.UpdateStatusWebView();
			}
			public void UpdateModsWebView() {
				Form.UpdateModsWebView();
			}
			public void CheckModUpdates() {
				Form.CheckModUpdates();
			}

			public void UncheckFile(int groupidx, int fileidx) {
				try {
					FileInfo fi = Program.LoncherSettings.GameVersion.FileGroups[groupidx].Files[fileidx];
					if (fi != null && fi.Importance > 0) {
						fi.IsCheckedToDl = false;
						Form.CheckReady();
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}
			public void CheckFile(int groupidx, int fileidx) {
				try {
					FileInfo fi = Program.LoncherSettings.GameVersion.FileGroups[groupidx].Files[fileidx];
					if (fi != null) {
						fi.IsCheckedToDl = true;
						Form.SetReady(false);
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}

			public bool LaunchGame() {
				Form.OnLaunchGameBtn();
				return true;
			}


			private List<WebModInfo> modList_;
			//private bool refreshModList_ = true;
			internal List<WebModInfo> ModList {
				get {
					//if (modList_ is null || refreshModList_) {
						modList_ = new List<WebModInfo>();
						for (int i = 0; i < Program.LoncherSettings.Mods.Count; i++) {
							ModInfo mod = Program.LoncherSettings.Mods[i];
							if (mod.CurrentVersionFiles != null) {
								modList_.Add(new WebModInfo(mod));
							}
						}
						//refreshModList_ = false;
					//}
					return modList_;
				}
			}

			private ModInfo getModInfoByIdx(int idx) {
				if (modList_ is null) {
					YobaDialog.ShowDialog("Mod List has not been initialized yet");
				}
				else if (idx < 0 || idx >= modList_.Count) {
					YobaDialog.ShowDialog("Invalid Mod List idx (" + idx + " while modlist length is " + modList_.Count + ")");
				}
				else {
					return modList_[idx].ModInfo;
				}
				return null;
			}

			private bool checkConflicts(ModInfo mi, string locKey) {
				List<string> conflictedMods = new List<string>();
				foreach (ModInfo ami in Program.LoncherSettings.AvailableMods) {
					if (ami.IsActive && ami.DoesConflict(mi)) {
						conflictedMods.Add(ami.VersionedName);
					}
				}
				if (conflictedMods.Count > 0) {
					if (DialogResult.Yes != YobaDialog.ShowDialog(
							String.Format(Locale.Get(locKey), string.Join("\r\n", conflictedMods))
							, YobaDialog.YesNoBtns)) {
						return false;
					}
				}
				return true;
			}
			private bool checkDependencies(ModInfo mi, string locKey) {
				List<string> dependentMods = new List<string>();
				foreach (ModInfo dmi in Program.LoncherSettings.AvailableMods) {
					if (dmi.IsActive && dmi.DoesDepend(mi)) {
						dependentMods.Add(dmi.VersionedName);
					}
				}
				if (dependentMods.Count > 0) {
					if (DialogResult.Yes != YobaDialog.ShowDialog(
							String.Format(Locale.Get(locKey), string.Join("\r\n", dependentMods))
							, YobaDialog.YesNoBtns)) {
						return false;
					}
				}
				return true;
			}

			public void ModInstall(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null && checkConflicts(mi, "SomeModsConflictWithThisInstall")) {
					InstallModAsync(mi);
				}
			}
			
			public void ModUninstall(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null && checkDependencies(mi, "SomeModsDependOnThisDelete")) {
					if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("AreYouSureUninstallMod"), mi.Name), YobaDialog.YesNoBtns)) {
						mi.Delete();
						Form.UpdateModsWebView();
					}
				}
			}
			public void ModDisable(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null && checkDependencies(mi, "SomeModsDependOnThisDisable")) {
					mi.Disable();
					Form.UpdateModsWebView();
				}
			}
			public void ModEnable(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null && checkConflicts(mi, "SomeModsConflictWithThisEnable")) {
					ModEnableAsync(mi);
				}
			}

			internal async void ModEnableAsync(ModInfo mi) {
				CheckResult modFileCheckResult = await mi.Enable();
				if (modFileCheckResult is null || modFileCheckResult.IsAllOk) {
					Form.UpdateModsWebView();
				}
				else {
					LinkedList<FileInfo> files = modFileCheckResult.InvalidFiles;
					uint size = 0;
					foreach (FileInfo fi in files) {
						size += fi.Size;
					}
					if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("ModActivationFilesAreOutdated"), mi.VersionedName, YU.formatFileSize(size)), YobaDialog.YesNoBtns)) {
						if (Form.modsToUpdate_ is null) {
							Form.modsToUpdate_ = new LinkedList<ModInfo>();
							Form.modsToUpdate_.AddLast(mi);
							mi.DlInProgress = true;
							Form.UpdateModsWebView();
							if (!Form.UpdateInProgress_) {
								Form.DownloadNextMod();
							}
						}
						else {
							mi.DlInProgress = true;
							Form.modsToUpdate_.AddLast(mi);
							Form.UpdateModsWebView();
						}
					}
					else {
						if (DialogResult.Yes == YobaDialog.ShowDialog(Locale.Get("ModDisableToPreventCorruption"), YobaDialog.YesNoBtns)) {
							mi.Disable();
						}
						Form.UpdateModsWebView();
					}
				}
			}

			internal async void InstallModAsync(ModInfo mi) {
				uint size = 0;
				if (mi.CurrentVersionFiles[0].Size == 0) {
					await FileChecker.CheckFiles(mi.CurrentVersionFiles);
				}
				foreach (FileInfo fi in mi.CurrentVersionFiles) {
					if (!fi.IsOK) {
						size += fi.Size;
					}
				}
				string modSize = YU.formatFileSize(size);
				if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("AreYouSureInstallMod"), mi.Name, modSize), YobaDialog.YesNoBtns)) {
					if (Form.modsToUpdate_ is null) {
						Form.modsToUpdate_ = new LinkedList<ModInfo>();
						Form.modsToUpdate_.AddLast(mi);
						mi.DlInProgress = true;
						Form.UpdateModsWebView();
						if (!Form.UpdateInProgress_) {
							Form.DownloadNextMod();
						}
					}
					else {
						mi.DlInProgress = true;
						Form.modsToUpdate_.AddLast(mi);
						Form.UpdateModsWebView();
					}
				}
			}

			/*
			 * OPTIONS
			 */
			public string OptionsGetCurrentSettings() {
				Dictionary<string, string> settings = new Dictionary<string, string>();
				settings.Add("CurrentlyOffline", Program.OfflineMode ? "1" : "0");
				settings.Add("StartOffline", LauncherConfig.StartOffline ? "1" : "0");
				settings.Add("CloseOnLaunch", LauncherConfig.CloseOnLaunch ? "1" : "0");
				settings.Add("LaunchFromGalaxy", LauncherConfig.LaunchFromGalaxy ? "1" : "0");
				settings.Add("ZoomPercent", LauncherConfig.ZoomPercent.ToString());
				settings.Add("LoggingLevel", LauncherConfig.LoggingLevel.ToString());
				settings.Add("GameDir", LauncherConfig.GameDir);
				settings.Add("StartPage", ((int)LauncherConfig.StartPage).ToString());
				return JsonConvert.SerializeObject(settings);
			}
			public bool OptionsCheckOffline(bool offlineOn) {
				LauncherConfig.StartOffline = offlineOn;
				if (Program.OfflineMode != offlineOn) {
					if (YobaDialog.ShowDialog(Locale.Get(offlineOn ? "OfflineModeSet" : "OnlineModeSet"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
						Form.Hide();
						new PreloaderForm(Form).Show();
					}
				}
				return LauncherConfig.StartOffline;
			}
			public bool OptionsCheckLaunchFromGalaxy(bool isChecked) {
				LauncherConfig.LaunchFromGalaxy = isChecked;
				return LauncherConfig.LaunchFromGalaxy;
			}
			public bool OptionsCheckCloseOnLaunch(bool isChecked) {
				LauncherConfig.CloseOnLaunch = isChecked;
				return LauncherConfig.CloseOnLaunch;
			}
			public int OptionsSelectStartPage(int pageId) {
				LauncherConfig.StartPage = (StartPageEnum)pageId;
				return (int)LauncherConfig.StartPage;
			}

			public void OptionsSetLoggingLevel(int level) {
				LauncherConfig.LoggingLevel = level;
			}
			public int OptionsSetZoom(int zoom) {
				return Form.SetBrowserZoom(zoom);
			}

			public string OptionsBrowseGamePath() {
				CommonOpenFileDialog folderBrowserDialog = new CommonOpenFileDialog() {
					IsFolderPicker = true
					, InitialDirectory = Program.GamePath
				};
				if (folderBrowserDialog.ShowDialog() == CommonFileDialogResult.Ok) {
					string path = folderBrowserDialog.FileName;
					if (path[path.Length - 1] != Path.DirectorySeparatorChar) {
						path += Path.DirectorySeparatorChar;
					}
					if (Program.GamePath != path) {
						if (File.Exists(path + Program.LoncherSettings.ExeName)) {
							LauncherConfig.GameDir = path;
							if (YobaDialog.ShowDialog(Locale.Get("GamePathChanged"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
								Form.Hide();
								new PreloaderForm(Form).Show();
							}
							return path;
						}
						else {
							YobaDialog.ShowDialog(Locale.Get("NoExeInPath"));
						}
					}
				}
				return "";
			}

			public void OptionsOpenDataFolder() {
				Process.Start(new ProcessStartInfo {
					Arguments = "/C explorer \"" + Program.GamePath + "data\""
					, FileName = "cmd"
					, WindowStyle = ProcessWindowStyle.Hidden
				});
			}

			public void OptionsUninstallRussifier() {
				try {
					UninstallationRules urules_ = Program.LoncherSettings.UninstallationRules;
					string msg = Locale.Get("ProductUninstallationConfirmation") + ":";
					foreach (FileInfo fi in urules_.FilesToDelete) {
						if (File.Exists(Program.GamePath + fi.Path)) {
							msg += "\r\n" + Program.GamePath + fi.Path;
						}
					}
					if (YobaDialog.ShowDialog(msg, YobaDialog.YesNoBtns) == DialogResult.Yes) {
						foreach (FileInfo fi in urules_.FilesToDelete) {
							if (File.Exists(Program.GamePath + fi.Path)) {
								File.Delete(Program.GamePath + fi.Path);
							}
						}
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}

			public void OptionsUninstallLoncher() {
				try {
					string msg = Locale.Get("LoncherUninstallationConfirmation");
					if (YobaDialog.ShowDialog(msg, YobaDialog.YesNoBtns) == DialogResult.Yes) {
						if (Directory.Exists(Program.LoncherDataPath)) {
							Directory.Delete(Program.LoncherDataPath, true);
						}
						Process.Start(new ProcessStartInfo {
							Arguments = String.Format("/C choice /C Y /N /D Y /T 1 & Del \"{0}\"", Application.ExecutablePath)
							, FileName = "cmd"
							, WindowStyle = ProcessWindowStyle.Hidden
						});
						Application.Exit();
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}

			public void OptionsCreateShortcut() {
				try {
					string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
						+ Path.DirectorySeparatorChar + Program.LoncherSettings.LoncherLinkName + ".lnk";
					
					if (File.Exists(filename)) {
						YobaDialog.ShowDialog(Locale.Get("ShortcutAlreadyExists"));
					}
					else {
						IWshRuntimeLibrary.WshShell wsh = new IWshRuntimeLibrary.WshShell();
						IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(filename) as IWshRuntimeLibrary.IWshShortcut;
						shortcut.Arguments = "";
						shortcut.TargetPath = Application.ExecutablePath;
						shortcut.WorkingDirectory = Program.LoncherPath;
						shortcut.WindowStyle = 1;
						string iconFile = Program.LoncherDataPath + "shortcutIcon.ico";
						bool validIconFile = File.Exists(iconFile);
						if (!validIconFile) {
							string exename = Program.GamePath + Program.LoncherSettings.ExeName;

							if (File.Exists(PreloaderForm.ICON_FILE)) {
								PngIconConverter.Convert(PreloaderForm.ICON_FILE, iconFile);
								validIconFile = true;
							}
							else if (File.Exists(exename) && exename.EndsWith(".exe")) {
								Icon exeIcon = Icon.ExtractAssociatedIcon(exename);
								if (exeIcon != null) {
									Bitmap exeBmp = exeIcon.ToBitmap();
									PngIconConverter.Convert(exeBmp, iconFile);
									validIconFile = true;
								}
							}
						}
						if (validIconFile) {
							shortcut.IconLocation = iconFile;
						}
						shortcut.Save();
						YobaDialog.ShowDialog(Locale.Get("ShortcutCreatedSuccessfully"));
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}

			public void OptionsMakeBackup() {
				string bkpdir = Program.GamePath + "_loncher_backups\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "\\";
				if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("SettingsMakeBackupInfo"), bkpdir), YobaDialog.YesNoBtns)) {
					try {
						string origDir = Program.GamePath;
						if (!Directory.Exists(bkpdir)) {
							Directory.CreateDirectory(bkpdir);
						}

						List<string> dirs = new List<string>();

						void backupFile(FileInfo fi) {
							string path = fi.Path.Replace('/', '\\');
							int fileNameStart = path.LastIndexOf('\\');
							if (fileNameStart > 0) {
								string dir = path.Substring(0, fileNameStart);
								if (!dirs.Contains(dir)) {
									if (!Directory.Exists(bkpdir + dir)) {
										Directory.CreateDirectory(bkpdir + dir);
									}
									dirs.Add(dir);
								}
							}
							if (File.Exists(origDir + path)) {
								File.Copy(origDir + path, bkpdir + path);
							}
						}

						GameVersion gameVersion = Program.LoncherSettings.GameVersion;
						foreach (FileGroup fg in gameVersion.FileGroups) {
							foreach (FileInfo fi in fg.Files) {
								backupFile(fi);
							}
						}
						foreach (FileInfo fi in gameVersion.Files) {
							backupFile(fi);
						}
						YobaDialog.ShowDialog(String.Format(Locale.Get("SettingsMakeBackupDone"), bkpdir));
					}
					catch (Exception ex) {
						YobaDialog.ShowDialog(ex.Message);
					}
				}
			}
		}
	}
}
