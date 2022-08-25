<?php
$test = false;

if (isset($_GET["test"])) {
	$test = $_GET["test"] == '1';
}
?>

<!DOCTYPE html>
<html>
<head>
<title>Yoba Löncher</title>
<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
<meta http-equiv="X-UA-Compatible" content="IE=11" />
<link type="text/css" rel="stylesheet" href="main.css" />
<script language='JavaScript' src='assets/jq' type='text/javascript'></script>
<script language='JavaScript' src='assets/ts' type='text/javascript'></script>
<script language='JavaScript' src='main.js' type='text/javascript'></script>

<?php
	echo $test ? "<script language='JavaScript' src='assets/yobalib_dummy.js' type='text/javascript'></script>" : "<script>;[[[YOBALIB]]];</script>";

	$links = file_get_contents('links.html');
	$headerContent = "";
	if ($links == false) {
		$links = "Ссылки отсутствуют";
	} else {
		$pageSplit = explode("<!---/HEAD---->", $links);
		if (count($pageSplit) > 1) {
			$headerContent = $pageSplit[0];
			$links = $pageSplit[1];
		}
	}
	echo $headerContent;
?>

<script>

<?php
	echo "var TESTMODE = ".($test ? "true" : "false").";";
	$changelog = file_get_contents('changelog.html');
	if ($changelog != false) {
		$changelog = str_replace('\\', '\\\\', $changelog);
		$changelog = str_replace("\n", "\\\n", $changelog);
		$changelog = str_replace("\r", "", $changelog);
		$changelog = str_replace('\'', '\\\'', $changelog);
		echo "var changelograw = '".$changelog."';";
	}
	else {
		echo "var changelograw = 'Ошибка\\\n=====Content=====\\\n<h1>Ошибка</h1><br><h2>Не удалось прочитать файл ченджлога</h2>';";
	}

	$faq = file_get_contents('faq.html');
	if ($faq != false) {
		$faq = str_replace('\\', '\\\\', $faq);
		$faq = str_replace("\n", "\\\n", $faq);
		$faq = str_replace("\r", "", $faq);
		$faq = str_replace('\'', '\\\'', $faq);
		echo "var faqraw = '".$faq."';";
	}
	else {
		echo "var faqraw = '<h1>Ошибка</h1><br><h2>Не удалось прочитать файл FAQ</h2>';";
	}
?>

var _START_VIEW_ID = "Status"

var mainLocale = YL.GetLocs('LaunchBtn, UpdateBtn, Close, Minimize, Help, About'
+ ', ChangelogBtn, LinksBtn, StatusBtn, SettingsBtn, FAQBtn, ModsBtn, ChangelogTooltip, StatusTooltip, LinksTooltip, SettingsTooltip, ModsTooltip, FAQTooltip')

var modsLocale = YL.GetLocs('ModInstallationInProgress, InstallMod, EnableMod, DisableMod, UninstallMod, NoModsForThisVersion, ModDetailedInfo')

var statusLocale = YL.GetLocs('StatusListDownloadedFile, StatusListDownloadedFileTooltip, StatusListRecommendedFile, StatusListRecommendedFileTooltip,'
+ ', StatusListOptionalFile, StatusListOptionalFileTooltip, StatusListRequiredFile, StatusListOptionalFileTooltip'
+ ', StatusComboboxDownload, StatusComboboxDownloadForced, StatusComboboxNoDownload, StatusComboboxUpdate, StatusComboboxUpdateForced, StatusComboboxNoUpdate')

YL.On('ProgressBarUpdate', function(event) {
	progressBar.SetValue(event.Progress)
	if (typeof event.Text == 'string') {
		progressTitle.SetValue(event.Text)
	}
})

YL.On('LaunchButtonUpdate', function(event) {
	launchBtn.Update(event.IsReady, event.Enabled)
})

YL.On('ModsViewUpdate', function(modsList) {
	var modsContent = $('#ModsView .article-content')
	modsContent.empty()
	if (!modsList) {
		$("<div class='noMods'>").appendTo(modsContent).text("No Modlist provided")
	}
	else if (modsList.length) {
		modTabIdx = 50
		for (var i = 0; i < modsList.length; i++) {
			var modInfo = modsList[i]
			var modEntry = $("<div class='modEntry'>").appendTo(modsContent)
			var modControls = $("<div class='modControls'>")
			var modDetails = $("<div class='modDesc'>").text(modInfo.Description)
			modEntry[0].modIdx = i
			modEntry.append(
				$("<div class='modTitle'>").text(modInfo.Name)
				, modDetails
				, modControls
			)

			if (modInfo.Screenshots || modInfo.DetailedDescription) {
				$("<div class='modControlButton details'>").text(modsLocale["ModDetailedInfo"]).onActivate(function() {
					showModDetails(this.__modInfo)
				}).attr('tabindex', modTabIdx++).appendTo(modDetails)[0].__modInfo = modInfo
			}

			if (modInfo.DlInProgress) {
				modEntry.addClass('loading')
				$("<div class='modLoadingLabel'>").text(modsLocale["ModInstallationInProgress"]).appendTo(modControls)
			}
			else if (!modInfo.Installed) {
				createModBtn(modControls, 'Install')
			}
			else {
				if (modInfo.Active) {
					modEntry.addClass("active")
					createModBtn(modControls, 'Disable')
				}
				else {
					modEntry.addClass("inactive")
					createModBtn(modControls, 'Enable')
				}
				modEntry.addClass("installed")
				createModBtn(modControls, 'Uninstall')
			}
		}
	}
	else {
		$("<div class='noMods'>").appendTo(modsContent).text(modsLocale["NoModsForThisVersion"])
	}
});

var modTabIdx = 50, statusTabIdx = 11;
function createModBtn(modControls, key) {
	$("<div class='modControlButton " + key.toLowerCase() + "'>").attr('tabindex', modTabIdx++).text(modsLocale[key + "Mod"]).appendTo(modControls).onActivate(function() {
		var idx = $(this).parents('.modEntry')[0].modIdx
		YL['Mod' + key](idx)
	})
}

function onFileActionComboboxSelect() {
	if (!$('#app-content-pages').hasClass("disabled")) {
		var groupidx = parseInt(this.FileElement.attr('groupidx'))
		var fileidx = parseInt(this.FileElement.attr('fileidx'))
		if (this.Value) {
			YL.CheckFile(groupidx, fileidx);
		} else {
			YL.UncheckFile(groupidx, fileidx);
		}
	}
}

YL.On('StatusViewUpdate', function(gameVersion) {
	var statusContent = $('#StatusView .article-content')
	statusContent.empty()
	if (!gameVersion) {
		$("<div class='noMods'>").appendTo(statusContent).text("Нет данных о версии игры. Скорее всего, русификатор недоступен для вашей версии игры.")
	}
	else {// if (gameVersion.length) {
		function appendFiles(container$, files, groupIdx) {
			for (var i = 0; i < files.length; i++) {
				var fi = files[i]
				var tooltip = fi.Tooltip
				var fileElement = $("<div class='fileEntry' fileidx='" + i + "'>").appendTo(container$)
				if (fi.IsOK) {
					fileElement.addClass("fileok")
					statusText = statusLocale["StatusListDownloadedFile"]
					if (!tooltip) {
						tooltip = statusLocale["StatusListDownloadedFileTooltip"]
					}
					$("<div class='statusIndicator'>").text(statusText).appendTo(fileElement)
				}
				else {
					var options;
					if (!tooltip) {
						if (fi.Importance === 1 && fi.IsPresent) {
							tooltip = statusLocale["StatusListRecommendedFileTooltip"]
						}
						else if (fi.Importance > 1) {
							tooltip = statusLocale["StatusListOptionalFileTooltip"]
						}
						else {
							tooltip = statusLocale["StatusListRequiredFileTooltip"]
						}
					}
					if (fi.IsPresent) {
						if (fi.Importance > 0) {
							options = [{ Text: statusLocale['StatusComboboxUpdate'], Value: true }, { Text: statusLocale['StatusComboboxNoUpdate'], Value: false }]
						}
						else {
							options = [{ Text: statusLocale['StatusComboboxUpdateForced'], Value: true }]
						}
					}
					else {
						if (fi.Importance > 1) {
							options = [{ Text: statusLocale['StatusComboboxDownload'], Value: true }, { Text: statusLocale['StatusComboboxNoDownload'], Value: false }]
						}
						else {
							options = [{ Text: statusLocale['StatusComboboxDownloadForced'], Value: true }]
						}
					}
					var fileActionCombobox = $("<div class='fileActionCombobox' fileidx='" + i + "' groupidx='" + groupIdx + "'>").appendTo(fileElement)

					var cb = new StyledComboBox(fi.IsCheckedToDl ? 0 : 1, options)
					cb.Container.appendTo(fileActionCombobox)
					if (options.length < 2) {
						cb.Disable()
					} else {
						cb.FileElement = fileActionCombobox
						cb.OnSelect = onFileActionComboboxSelect
						cb.SetTabIndex(statusTabIdx++)
					}
				}
				fileElement.attr('title', tooltip).append(
					$("<div class='fileName'>").text(fi.Description || fi.Path)
				);
			}
		}
		try {
			statusTabIdx = 11
			for (var i = 0; i < gameVersion.FileGroups.length; i++) {
				var fg = gameVersion.FileGroups[i]
				var spoiler = $("<div class='group-spoiler-button'>").appendTo(statusContent).click(function() {
					var tsc = touchScrollControllers['Status']
					if (this._expanded = !this._expanded) {
						this._spoilerContent.slideDown(150, function() { tsc.TouchScrollCheck() })
						this._spoilerIndicator.html('-&nbsp;')
					} else {
						this._spoilerContent.slideUp(150, function() { tsc.TouchScrollCheck() })
						this._spoilerIndicator.html('+&nbsp;')
					}
				})
				spoiler[0]._spoilerIndicator = $("<span class='spoilersym'>-&nbsp;</span>").appendTo(spoiler)
				$("<span class='group-name'>").text(fg.Name).appendTo(spoiler)

				spoiler[0]._spoilerContent = $("<div class='group-spoiler'>").appendTo(statusContent)
				spoiler[0]._expanded = true
				appendFiles(spoiler[0]._spoilerContent, fg.Files, i);
				$("<div class='spoilerdash'>").appendTo(statusContent)
			}
			if (gameVersion.Files.length) {
				appendFiles($("<div class='version-files'>").appendTo(statusContent), gameVersion.Files, -1);
			}
		}
		catch (ex) {
			$("<div class='noMods'>").appendTo(statusContent).text(ex)
		}
	}
});

</script>

</head>
<body>

<div class='progress-status'><div class='caption'></div></div>

<div class='progress'>
	<div class="border"><div class="border-mid"></div></div>
	<div class="bar">
		<div class="start"></div>
		<div class="mid"></div>
		<div class="end"></div>
	</div>
</div>

<div id='LaunchBtn' class='app-btn' tabindex='1'><div class='caption'></div></div>

<div class='app-buttons'>
</div>

<div class='app-content-pages'>
	<div class='page' id='ChangelogView'>

		<div class='kb-side-menu'></div>

		<div class="articleContentWrapper">
			<div class='article-content'></div>
		</div>

	</div>

	<div class='page' id='ModsView'>
		<div class="articleContentWrapper">
			<div class='article-content'></div>
		</div>
	</div>

	<div class='page' id='FAQView'>
		<div class="articleContentWrapper">
			<div class='article-content'></div>
		</div>
	</div>

	<div class='page' id='LinksView'>
		<div class="articleContentWrapper">
			<div class='article-content'><?php echo $links; ?></div>
		</div>
	</div>

	<div class='page' id='StatusView'>
		<div class="articleContentWrapper">
			<div class='article-content'><h2>Подготовка...</h2></div>
		</div>
	</div>

	<div class='page' id='SettingsView'>
		<div class='articleContentWrapper'>
			<div class='article-content'></div>
		</div>
	</div>
</div>

<div class='preloader'></div>
<div class='app-controls'>
	<div class='app-control-btn close' onclick="YL.AppClose()" tabindex='212'></div>
	<div class='app-control-btn minimize' onclick="YL.AppMinimize()" tabindex='211'></div>
	<div class='app-control-btn help' onclick="YL.AppHelp()" tabindex='210'></div>
</div>

</body>
</html>