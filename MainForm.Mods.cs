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

		private async Task<bool> FinalizeModDownload(FileInfo lastFileInfo) {
			bool success = false;
			ModInfo modInfo = lastFileInfo.LastFileOfMod ?? lastFileInfo.LastFileOfModToUpdate;
			await Task.Run(() => {
				string filename = "";
				try {
					List<string> failedFiles = new List<string>();
					LinkedListNode<FileInfo> currentMod = modFilesToUpload_.First;
					bool gotLast = false;
					while (!gotLast && currentMod != null) {
						gotLast = currentMod.Value == lastFileInfo;
						if (currentMod.Value.IsOK) {
							continue;
						}
						filename = ThePath + currentMod.Value.Path.Replace('/', '\\');
						string errorStr = MoveUploadedFile(filename, currentMod.Value);
						if (errorStr != null) {
							failedFiles.Add(errorStr);
						}
						currentMod = currentMod.Next;
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

		private async void DownloadNextMod() {
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

		private void FinishModDownload() {
			currentFile_ = null;
			modFilesToUpload_ = null;
			foreach (ModInfo mi in Program.LoncherSettings.Mods) {
				mi.DlInProgress = false;
			}
			LaunchButtonEnabled_ = true;
			UpdateModsWebView();
			CheckReady();
		}
	}
}