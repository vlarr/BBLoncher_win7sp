using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace YobaLoncher {
	public partial class MainForm {

		private string MoveUploadedFile(string filename, FileInfo fileInfo) {
			string dirpath = filename.Substring(0, filename.LastIndexOf('\\'));
			Directory.CreateDirectory(dirpath);
			if (File.Exists(filename)) {
				File.Delete(filename);
			}
			File.Move(PreloaderForm.UPDPATH + fileInfo.UploadAlias, filename);

			fileInfo.IsPresent = true;
			string md5 = FileChecker.GetFileMD5(filename);
			if (fileInfo.Hashes != null && fileInfo.Hashes.Count > 0 && null == fileInfo.Hashes.Find(x => x.Equals(md5, StringComparison.InvariantCultureIgnoreCase))) {
				return fileInfo.Path + " : " + md5;
			}

			fileInfo.IsOK = true;
			LauncherConfig.FileDates[fileInfo.Path] = YU.GetFileDateString(filename);
			LauncherConfig.FileDateHashes[fileInfo.Path] = md5;
			return null;
		}

		private async Task<bool> FinalizeModDownload(ModInfo modInfo) {
			List<FileInfo> files = modInfo.CurrentVersionFiles;
			int progressStep = progressBarInfo_.MaxValue / files.Count;
			bool success = false;
			string filename = "";
			try {
				List<string> failedFiles = new List<string>();
				for (int i = 0; i < files.Count; i++) {
					UpdateProgressBar(progressStep * i);
					FileInfo fi = files[i];
					if (fi.IsOK) {
						continue;
					}
					filename = ThePath + fi.Path.Replace('/', '\\');
					string errorStr = await Task<string>.Run(() => {
						return MoveUploadedFile(filename, fi);
					});
					if (errorStr != null) {
						failedFiles.Add(errorStr);
					}
				}
				modInfo.Install();
				success = true;
				if (failedFiles.Count > 0) {
					YobaDialog.ShowDialog(String.Format(Locale.Get("UpdateModHashCheckFailed"), String.Join("\r\n", failedFiles)));
				}
			}
			catch (UnauthorizedAccessException ex) {
				ShowDownloadError(string.Format(Locale.Get("DirectoryAccessDenied"), filename) + ":\r\n" + ex.Message);
			}
			catch (Exception ex) {
				ShowDownloadError(string.Format(Locale.Get("CannotMoveFile"), filename) + ":\r\n" + ex.Message);
			}
			modInfo.DlInProgress = false;
			UpdateModsWebView();
			return success;
		}

		private LinkedListNode<ModInfo> currentMod_ = null;

		private async void DownloadNextMod() {
			if (currentMod_ is null) {
				if (modsToUpdate_ == null || modsToUpdate_.Count < 1) {
					FinishModDownload();
					return;
				}
				LaunchButtonEnabled_ = false;
				UpdateLaunchButton();
				currentMod_ = modsToUpdate_.First;
				downloadProgressTracker_ = new DownloadProgressTracker(50, TimeSpan.FromMilliseconds(500));
			}
			else {
				currentMod_ = currentMod_.Next;
			}

			if (currentMod_ != null) {
				foreach (FileInfo fi in currentMod_.Value.CurrentVersionFiles) {
					if (!fi.IsOK && YU.stringHasText(fi.Url)) {
						DownloadFile(fi);
					}
				}
				UpdateProgressBar(0, Locale.Get("StatusCopyingFiles") + " // " + currentMod_.Value.VersionedName);
				if (await FinalizeModDownload(currentMod_.Value)) {
					DownloadNextMod();
				}
				else {
					UpdateProgressBar(0, Locale.Get("ModInstallationError"));
					FinishModDownload();
				}
			}
			else {
				UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("ModInstallationDone"));
				FinishModDownload();
			}
		}

		private void FinishModDownload() {
			currentFile_ = null;
			currentMod_ = null;
			modsToUpdate_ = null;
			foreach (ModInfo mi in Program.LoncherSettings.Mods) {
				mi.DlInProgress = false;
			}
			LaunchButtonEnabled_ = true;
			UpdateModsWebView();
			CheckReady();
		}

		/*private async Task<bool> FinalizeModDownload_old(FileInfo lastFileInfo) {
			bool success = false;
			ModInfo modInfo = lastFileInfo.LastFileOfMod ?? lastFileInfo.LastFileOfModToUpdate;
			await Task.Run(() => {
				string filename = "";
				try {
					List<string> failedFiles = new List<string>();
					LinkedListNode<FileInfo> currentModFileNode = modFilesToUpload_.First;
					bool gotLast = false;
					while (!gotLast && currentModFileNode != null) {
						gotLast = currentModFileNode.Value == lastFileInfo;
						if (currentModFileNode.Value.IsOK) {
							continue;
						}
						filename = ThePath + currentModFileNode.Value.Path.Replace('/', '\\');
						string errorStr = MoveUploadedFile(filename, currentModFileNode.Value);
						if (errorStr != null) {
							failedFiles.Add(errorStr);
						}
						currentModFileNode = currentModFileNode.Next;
						modFilesToUpload_.RemoveFirst();
					}
					lastFileInfo.LastFileOfModToUpdate = null;
					modInfo.Install();
					success = true;
					if (failedFiles.Count > 0) {
						YobaDialog.ShowDialog(String.Format(Locale.Get("UpdateModHashCheckFailed"), String.Join("\r\n", failedFiles)));
					}
				}
				catch (UnauthorizedAccessException ex) {
					ShowDownloadError(string.Format(Locale.Get("DirectoryAccessDenied"), filename) + ":\r\n" + ex.Message);
				}
				catch (Exception ex) {
					ShowDownloadError(string.Format(Locale.Get("CannotMoveFile"), filename) + ":\r\n" + ex.Message);
				}
			});
			modInfo.DlInProgress = false;
			UpdateModsWebView();
			return success;
		}

		private async void DownloadNextMod_old() {
			if (currentFile_ is null) {
				LaunchButtonEnabled_ = false;
				UpdateLaunchButton();
				currentFile_ = modFilesToUpload_.First;
				downloadProgressTracker_ = new DownloadProgressTracker(50, TimeSpan.FromMilliseconds(500));
			}
			else if (currentFile_.Value.LastFileOfMod != null || currentFile_.Value.LastFileOfModToUpdate != null) {
				UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("StatusCopyingFiles"));
				if (!await FinalizeModDownload(currentFile_.Value)) {
					FinishModDownload();
					return;
				}
				currentFile_ = modFilesToUpload_.First;
			}
			else {
				currentFile_ = currentFile_.Next;
			}
			if (currentFile_ != null) {
				if (currentFile_.Value.IsOK) {
					DownloadNextMod();
				}
				else {
					DownloadFile(currentFile_.Value);
				}
			}
			else {
				UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("ModInstallationDone"));
				FinishModDownload();
			}
		}

		private void FinishModDownload_old() {
			currentFile_ = null;
			modFilesToUpload_ = null;
			foreach (ModInfo mi in Program.LoncherSettings.Mods) {
				mi.DlInProgress = false;
			}
			LaunchButtonEnabled_ = true;
			UpdateModsWebView();
			CheckReady();
		}*/
	}
}