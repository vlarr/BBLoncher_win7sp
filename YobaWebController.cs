using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
					Form.draggingPanel.Width = Form.Width - ww - 4;
				}
				if (Int32.TryParse(height, out int hh)) {
					Form.draggingPanel.Height = hh;
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

			public void ModInstall(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null) {
					InstallModAsync(mi);
				}
			}
			
			public void ModUninstall(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null) {
					if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("AreYouSureUninstallMod"), mi.Name), YobaDialog.YesNoBtns)) {
						mi.Delete();
						Form.UpdateModsWebView();
					}
				}
			}
			public void ModDisable(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null) {
					mi.Disable();
					Form.UpdateModsWebView();
				}
			}
			public void ModEnable(int idx) {
				ModInfo mi = getModInfoByIdx(idx);
				if (mi != null) {
					try {
						mi.Enable();
					}
					catch (Exception ex) {
						YobaDialog.ShowDialog(String.Format(Locale.Get("CannotEnableMod"), mi.Name) + "\r\n\r\n" + ex.Message);
					}
					Form.UpdateModsWebView();
				}
			}

			internal async Task InstallModAsync(ModInfo mi) {
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
					if (Form.modFilesToUpload_ is null) {
						Form.modFilesToUpload_ = new LinkedList<FileInfo>(mi.CurrentVersionFiles);
						mi.DlInProgress = true;
						Form.UpdateModsWebView();
						if (!Form.UpdateInProgress_) {
							Form.DownloadNextMod();
						}
					}
					else {
						foreach (FileInfo fi in mi.CurrentVersionFiles) {
							Form.modFilesToUpload_.AddLast(fi);
						}
						mi.DlInProgress = true;
						Form.UpdateModsWebView();
					}
				}
			}
		}
	}
}
