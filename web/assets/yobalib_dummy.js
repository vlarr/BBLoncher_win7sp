

var YL = new (function () {

	this.GetLoc = function (key) {
		return _LOCALE[key]
	}
	this.GetLocs = function (keys) {
		return _LOCALE
	}

	var precachedLocale = {}
	var _self = this
	var _handlers = {}
	var __buffunc;

	this.__fire = function (eventName, data) {
		if (_handlers.hasOwnProperty(eventName)) {
			var hs = _handlers[eventName]
			for (var i = 0; i < hs.length; hs[i++](data));
		}
	}
	this.On = function (eventName, handler) {
		if (!_handlers.hasOwnProperty(eventName)) {
			_handlers[eventName] = []
		}
		_handlers[eventName].push(handler)
	}

	function setBufFunc() {
		__buffunc = {}
		for (var i = 0; i < arguments.length; i += 2) {
			__buffunc[arguments[i]] = arguments[i + 1]
		}
	}
	function runBufFunc(funcName) {
		if (__buffunc) {
			if (__buffunc.hasOwnProperty(funcName)) {
				__buffunc[funcName]()
				__buffunc = null
			}
			else {
				_self.Warn('Вызов отсутствующей буферной функции "' + funcName + '". Сообщите об этом происшествии авторам лаунчера.')
			}
		}
		else {
			_self.Warn('Вызов буферной функции при пустом буфере. Сообщите об этом происшествии авторам лаунчера.')
		}
	}
	this.__runBufFunc = function (funcName) {
		runBufFunc(funcName)
	}

	this.Warn = this.Info = this.ShowMessage = function () {
		switch (arguments.length) {
			case 1:
				alert(arguments[0])
				break
			case 2:
				alert(arguments[0])
				arguments[1]()
				break
			case 3:
				arguments[confirm(arguments[0]) ? 1 : 2]()
				break
		}
	}

	this.CheckFile = this.UncheckFile = this.AppClose = this.AppMinimize = this.AppHelp = function () {
		return true
	}

	this.ModInstall = function (idx) {
		//window.external.ModInstall(idx)
	}
	this.ModUninstall = function (idx) {
		//window.external.ModUninstall(idx)
	}
	this.ModEnable = function (idx) {
		//window.external.ModEnable(idx)
	}
	this.ModDisable = function (idx) {
		//window.external.ModDisable(idx)
	}

	this.RetrieveBackground = function () {
		return 'images/loncherbg0.jpg'
	}
	this.GetProgressBarState = function () {
		return { MaxValue: 1000, Progress: 500, Caption: "Типа работает"}
	}

	this.RetrieveStartViewId = this.UpdateAppControlsSize = function () {
		return null
	}

	this.UpdateModsData = function () {
		YL.__fire('ModsViewUpdate', [
			{
				"DlInProgress":false
				,"Installed":false
				,"Active":false
				,"Name":"True Balance / Истинный баланс в.2.134"
				,"Description":"Большое количество мелких изменений для более сбалансированного и разнообразного игрового процесса, с некоторым усилением многих врагов к середине и концу игры."
			}, {
				"DlInProgress":true
				,"Installed":false
				,"Active":false
				,"Name":"True Balance (Champions) в.2.134"
				,"Description":"В отличие от обычной версии, именные предметы в локациях заменены на 5000 крон. Шанс появления чемпионов увеличен на 5%. Именные предметы можно купить или получить с чемпионов."
			}, {
				"DlInProgress":false
				,"Installed":true
				,"Active":true
				,"Name":"Тестовая херня 1"
				,"Description":"Писька."
			}, {
				"DlInProgress":false
				,"Installed":true
				,"Active":false
				,"Name":"Тестовая херня 2"
				,"Description":"Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?"
			}
		])
	}
	this.UpdateStatusData = function () {
		var result = `{"GameVersion":{"ExeVersion":"1.4.0.44","Files":[],"FileGroups":[{"Name":"Файлы официальных DLC","Files":[{"Url":"https://koshk.ru/battlebrothers/data/data_002.dat","Path":"data/data_002.dat","Description":"Дополнение Fangshire (data_002.dat)","Tooltip":null,"Hashes":["F7DD326B0E17D3E48243A938E13833C0"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_003.dat","Path":"data/data_003.dat","Description":"Дополнение Lindwurm (data_003.dat)","Tooltip":null,"Hashes":["462A45C3C09B4B1EE0A951382B1531D0"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":2,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_004.dat","Path":"data/data_004.dat","Description":"Дополнение Beasts and Exploration (data_004.dat)","Tooltip":null,"Hashes":["B8438CBA8A77328E6DFB4F73A2C801DF"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":3,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_005.dat","Path":"data/data_005.dat","Description":"Дополнение Beasts and Exploration SE (data_005.dat)","Tooltip":null,"Hashes":["9CEC4D6D8C097F0548518D5BFC79D52B"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":1,"OrderIndex":4,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_006.dat","Path":"data/data_006.dat","Description":"Дополнение Warriors of the North (data_006.dat)","Tooltip":null,"Hashes":["DDB7BA3BD7F9914C0C955741C2F67782","9F2462A6D11B8270D48F4D2D136AFC5F"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":5,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_007.dat","Path":"data/data_007.dat","Description":"Дополнение Warriors of the North SE (data_007.dat)","Tooltip":null,"Hashes":["28e8765543fc69d139fd66c875604910"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":1,"OrderIndex":6,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_008_v15.dat","Path":"data/data_008.dat","Description":"Дополнение Blazing Deserts (data_008.dat)","Tooltip":null,"Hashes":["71236AF137EE6597BAE37E695BFD67BD"],"IsOK":false,"IsPresent":false,"UploadAlias":null,"Size":155163776,"Importance":0,"OrderIndex":7,"IsCheckedToDl":true},{"Url":"https://koshk.ru/battlebrothers/data/data_009.dat","Path":"data/data_009.dat","Description":"Дополнение Blazing Deserts SE (data_009.dat)","Tooltip":null,"Hashes":["B3139DBEC6298473F424DC6326B4D348"],"IsOK":false,"IsPresent":true,"UploadAlias":null,"Size":1522,"Importance":1,"OrderIndex":8,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_010.dat","Path":"data/data_010.dat","Description":"Дополнение Of Flesh and Faith (data_010.dat)","Tooltip":null,"Hashes":["12BC7AD1C78F9FF7459FDC82F6FFC975"],"IsOK":false,"IsPresent":false,"UploadAlias":null,"Size":2078,"Importance":0,"OrderIndex":9,"IsCheckedToDl":true}],"OrderIndex":0,"Collapsed":false},{"Name":"Файлы русификатора","Files":[{"Url":"https://koshk.ru/battlebrothers/data/15010/data_014.dat","Path":"data/data_014.dat","Description":"Русификатор (data_014.dat)","Tooltip":null,"Hashes":["A64EDB1E66BBF93EF9DC34F4DE30F443"],"IsOK":false,"IsPresent":true,"UploadAlias":null,"Size":19464594,"Importance":0,"OrderIndex":0,"IsCheckedToDl":true},{"Url":"https://koshk.ru/battlebrothers/data/15009/data_022.dat","Path":"data/data_022.dat","Description":"Русификатор карты (data_022.dat)","Tooltip":null,"Hashes":["DFEB50F473B8AAFC4DDC1D029B253D89"],"IsOK":false,"IsPresent":false,"UploadAlias":null,"Size":461478,"Importance":2,"OrderIndex":0,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/Cinzel-Black.ttf","Path":"data/gfx/fonts/cinzel/Cinzel-Black.ttf","Description":"Шрифт Cinzel-Black","Tooltip":null,"Hashes":["8D1E1ED63B2AFBC1AF7FB26BC5CCE564"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/Cinzel-Bold.ttf","Path":"data/gfx/fonts/cinzel/Cinzel-Bold.ttf","Description":"Шрифт Cinzel-Bold","Tooltip":null,"Hashes":["32B4A99705E61E27A8D78D7A153B7807"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/Cinzel-Regular.ttf","Path":"data/gfx/fonts/cinzel/Cinzel-Regular.ttf","Description":"Шрифт Cinzel-Regular","Tooltip":null,"Hashes":["77C2EF185CAE88FBCF3E8F195825D04B"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Bold-edited.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Bold-edited.ttf","Description":"Шрифт FreeUniversal-Bold-edited","Tooltip":null,"Hashes":["E1F2CB5497AB154081A5732659830388"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Bold.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Bold.ttf","Description":"Шрифт FreeUniversal-Bold","Tooltip":null,"Hashes":["35EDEDBB55603FD20C3717B699A43B82"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-BoldItalic.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-BoldItalic.ttf","Description":"Шрифт FreeUniversal-BoldItalic","Tooltip":null,"Hashes":["2FFA086B1FC1CD93948654AC5F360987"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Italic-edited.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Italic-edited.ttf","Description":"Шрифт FreeUniversal-Italic-edited","Tooltip":null,"Hashes":["C2638C4AE0E452A7B7C8EC81CF1BBF48"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Italic.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Italic.ttf","Description":"Шрифт FreeUniversal-Italic","Tooltip":null,"Hashes":["17A446575236DE87E41C1BE4A4DE9EDD"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Regular-edited.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Regular-edited.ttf","Description":"Шрифт FreeUniversal-Regular-edited","Tooltip":null,"Hashes":["AC6D8A59DAA09654BACBA05D426EB640"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Regular.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Regular.ttf","Description":"Шрифт FreeUniversal-Regular","Tooltip":null,"Hashes":["EB4CB4A5A484707B07C16FC834A704E1"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false}],"OrderIndex":2,"Collapsed":false}],"Name":null,"Description":null},"LaunchBtn":{"IsReady":false,"Enabled":false}}`;
		var data = JSON.parse(result)
		YL.__fire('LaunchButtonUpdate', data.LaunchBtn)
		YL.__fire('StatusViewUpdate', data.GameVersion)
		
		/*YL.__fire('StatusViewUpdate', {"ExeVersion":"1.4.0.44","Files":[],"FileGroups":[{"Name":"Файлы официальных DLC","Files":[{"Url":"https://koshk.ru/battlebrothers/data/data_002.dat","Path":"data/data_002.dat","Description":"Дополнение Fangshire (data_002.dat)","Tooltip":null,"Hashes":["F7DD326B0E17D3E48243A938E13833C0"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_003.dat","Path":"data/data_003.dat","Description":"Дополнение Lindwurm (data_003.dat)","Tooltip":null,"Hashes":["462A45C3C09B4B1EE0A951382B1531D0"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":2,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_004.dat","Path":"data/data_004.dat","Description":"Дополнение Beasts and Exploration (data_004.dat)","Tooltip":null,"Hashes":["B8438CBA8A77328E6DFB4F73A2C801DF"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":3,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_005.dat","Path":"data/data_005.dat","Description":"Дополнение Beasts and Exploration SE (data_005.dat)","Tooltip":null,"Hashes":["9CEC4D6D8C097F0548518D5BFC79D52B"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":1,"OrderIndex":4,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_006.dat","Path":"data/data_006.dat","Description":"Дополнение Warriors of the North (data_006.dat)","Tooltip":null,"Hashes":["DDB7BA3BD7F9914C0C955741C2F67782","9F2462A6D11B8270D48F4D2D136AFC5F"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":5,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_007.dat","Path":"data/data_007.dat","Description":"Дополнение Warriors of the North SE (data_007.dat)","Tooltip":null,"Hashes":["28e8765543fc69d139fd66c875604910"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":1,"OrderIndex":6,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_008_v15.dat","Path":"data/data_008.dat","Description":"Дополнение Blazing Deserts (data_008.dat)","Tooltip":null,"Hashes":["71236AF137EE6597BAE37E695BFD67BD"],"IsOK":false,"IsPresent":true,"UploadAlias":null,"Size":155163776,"Importance":0,"OrderIndex":7,"IsCheckedToDl":true},{"Url":"https://koshk.ru/battlebrothers/data/data_009.dat","Path":"data/data_009.dat","Description":"Дополнение Blazing Deserts SE (data_009.dat)","Tooltip":null,"Hashes":["B3139DBEC6298473F424DC6326B4D348"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":1,"OrderIndex":8,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/data_010.dat","Path":"data/data_010.dat","Description":"Дополнение Of Flesh and Faith (data_010.dat)","Tooltip":null,"Hashes":["12BC7AD1C78F9FF7459FDC82F6FFC975"],"IsOK":false,"IsPresent":false,"UploadAlias":null,"Size":2078,"Importance":0,"OrderIndex":9,"IsCheckedToDl":true}],"OrderIndex":0,"Collapsed":false},{"Name":"Файлы русификатора","Files":[{"Url":"https://koshk.ru/battlebrothers/data/15010/data_014.dat","Path":"data/data_014.dat","Description":"Русификатор (data_014.dat)","Tooltip":null,"Hashes":["D68FE8656B74363B3C612C4E60E876DC"],"IsOK":false,"IsPresent":true,"UploadAlias":null,"Size":19446316,"Importance":0,"OrderIndex":0,"IsCheckedToDl":true},{"Url":"https://koshk.ru/battlebrothers/data/15009/data_022.dat","Path":"data/data_022.dat","Description":"Русификатор карты (data_022.dat)","Tooltip":null,"Hashes":["DFEB50F473B8AAFC4DDC1D029B253D89"],"IsOK":false,"IsPresent":false,"UploadAlias":null,"Size":461478,"Importance":2,"OrderIndex":0,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/Cinzel-Black.ttf","Path":"data/gfx/fonts/cinzel/Cinzel-Black.ttf","Description":"Шрифт Cinzel-Black","Tooltip":null,"Hashes":["8D1E1ED63B2AFBC1AF7FB26BC5CCE564"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/Cinzel-Bold.ttf","Path":"data/gfx/fonts/cinzel/Cinzel-Bold.ttf","Description":"Шрифт Cinzel-Bold","Tooltip":null,"Hashes":["32B4A99705E61E27A8D78D7A153B7807"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/Cinzel-Regular.ttf","Path":"data/gfx/fonts/cinzel/Cinzel-Regular.ttf","Description":"Шрифт Cinzel-Regular","Tooltip":null,"Hashes":["77C2EF185CAE88FBCF3E8F195825D04B"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Bold-edited.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Bold-edited.ttf","Description":"Шрифт FreeUniversal-Bold-edited","Tooltip":null,"Hashes":["E1F2CB5497AB154081A5732659830388"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Bold.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Bold.ttf","Description":"Шрифт FreeUniversal-Bold","Tooltip":null,"Hashes":["35EDEDBB55603FD20C3717B699A43B82"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-BoldItalic.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-BoldItalic.ttf","Description":"Шрифт FreeUniversal-BoldItalic","Tooltip":null,"Hashes":["2FFA086B1FC1CD93948654AC5F360987"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Italic-edited.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Italic-edited.ttf","Description":"Шрифт FreeUniversal-Italic-edited","Tooltip":null,"Hashes":["C2638C4AE0E452A7B7C8EC81CF1BBF48"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Italic.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Italic.ttf","Description":"Шрифт FreeUniversal-Italic","Tooltip":null,"Hashes":["17A446575236DE87E41C1BE4A4DE9EDD"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Regular-edited.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Regular-edited.ttf","Description":"Шрифт FreeUniversal-Regular-edited","Tooltip":null,"Hashes":["AC6D8A59DAA09654BACBA05D426EB640"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false},{"Url":"https://koshk.ru/battlebrothers/data/fonts/FreeUniversal-Regular.ttf","Path":"data/gfx/fonts/free_universal/FreeUniversal-Regular.ttf","Description":"Шрифт FreeUniversal-Regular","Tooltip":null,"Hashes":["EB4CB4A5A484707B07C16FC834A704E1"],"IsOK":true,"IsPresent":true,"UploadAlias":null,"Size":0,"Importance":0,"OrderIndex":1,"IsCheckedToDl":false}],"OrderIndex":2,"Collapsed":false}],"Name":null,"Description":null});
	
		YL.__fire('LaunchButtonUpdate', {"IsReady":true,"Enabled":true})*/
	}
})();

/*$(function() {
	$.ajax({
		url: "https://koshk.ru/battlebrothers/assets/locale_default.txt"
		, success: function(data) {
			if (data) {
				data = data.replace('\r', '').split('\n')
				for (var i = 0; i < data.length; i++) {
					var di = data[i]
					var eqidx = di.indexOf('=')
					if (eqidx > -1) {
						_LOCALE[di.substring(0, eqidx).trim()] = di.substring(eqidx + 1).trim()
					}
				}
			}
			else alert("cannot get loc.txt")
		}
		, error: function(xr, errortext, exception) {
			alert(errortext + ":\r\n" + exception)
		}
	})
})*/


var _LOCALE = `
LaunchBtn = Запустить!
UpdateBtn = Обновить
OK = OK
Yes = Да
No = Нет
Done = Готово
Proceed = Продолжить
Cancel = Отмена
Close = Закрыть
Minimize = Свернуть
Help = Помощь
About = О программе
Apply = Применить
Quit = Выйти
ExitApp = Закрыть программу
Ignore = Игнорировать
CopyStackTrace = Скопировать информацию об ошибке
Retry = Повторить
RunOffline = Оффлайн-режим
Browse = Обзор...
ChangelogBtn = История изменений
LinksBtn = Полезные ссылки
StatusBtn = Состояние файлов
SettingsBtn = Настройки
FAQBtn = FAQ
FAQTooltip = FAQ
ChangelogTooltip = Показать список изменений
StatusTooltip = Показать результаты проверки файлов
LinksTooltip = Показать полезные ссылки
SettingsTooltip = Открыть настройки лаунчера
Error = Ошибка
LoncherLoading = Запускаем {0}...
PressF1About = Нажмите F1, чтобы открыть информацию о программе
DLRate = Загружаем: {3} - {0} из {1} @ {2}
AllFilesIntact = Все файлы прошли проверку, можно взлетать.
FilesMissing = Файлов, не прошедших проверку: {0} (они отсутствуют либо устарели)
StatusListDownloadedFile = 
StatusListRecommendedFile = 
StatusListOptionalFile = 
StatusListRequiredFile = 
StatusListDownloadedFileTooltip = Файл на месте и готов к работе
StatusListRecommendedFileTooltip = Файл не прошёл проверку версии, но для корректной работы это не так важно. Однако, если у вас всё же возникнут проблемы с игрой, то будет резонно начать с того, чтобы скачать рекомендуемую нами версию файла.
StatusListOptionalFileTooltip = Необязательный файл
StatusListRequiredFileTooltip = Файл либо отсутствует, либо имеет неподходящую версию. Его необходимо скачать для корректной работы игры и русификатора.
StatusComboboxDownload = Загрузить
StatusComboboxDownloadForced = Будет загружен
StatusComboboxNoDownload = Не загружать
StatusComboboxUpdate = Обновить
StatusComboboxUpdateForced = Будет обновлён
StatusComboboxNoUpdate = Не обновлять
StatusCopyingFiles = Обновляем файлы в папке игры...
StatusUpdatingDone = Готово!
StatusDownloadError = Произошла ошибка при загрузке или копировании файлов
CannotDownloadFile = Не удаётся скачать файл "{0}"
CannotMoveFile = Невозможно переместить файл "{0}"
DirectoryAccessDenied = Невозможно переместить файл "{0}": отказано в доступе. Попробуйте перезапустить лаунчер от имени администратора.[n][n]Конкретная ошибка
UpdateSuccessful = Все файлы успешно обновлены![n]Запустить игру сейчас?
LauncherIsInOfflineMode = Новости недоступны: лаунчер работает в оффлайн-режиме
ChangelogFileUnavailable = Файл истории изменений недоступен или некорректен
FAQFileUnavailable = Файл FAQ недоступен или некорректен
ModsBtn = Моды
ModsTooltip = Моды, доступные для данной версии игры
NoModsForThisVersion = Нет модов, доступных для вашей версии.
YouHaveOutdatedMods = Доступна новая версия для следующих модов: {0}[n][n]Обновить сейчас?[n](Будет скачано {1})
YouHaveAlteredMods = У этих модов изменилась структура файлов: {0}[n][n]Их крайне желательно обновить, иначе всё может сломаться. Обновить их сейчас?[n](Будет скачано {1})
InstallMod = Установить
UninstallMod = Удалить
EnableMod = Включить
DisableMod = Отключить
ModInstallationInProgress = Установка...
ModInstallationDone = Установка модов завершена
CannotEnableMod = Не удаётся активировать мод
AreYouSureInstallMod = Установить мод «{0}»?[n]Будет скачано {1} {2}
AreYouSureUninstallMod = Удалить мод «{0}»?
FollowingModsAreDisabled = Следующие моды будут отключены, т.к. они не поддерживают текущую версию игры:[n]{0}
FollowingModsMayBeEnabled = Со следующими модами вы ранее играли на этой версии. Включить их?[n]{0}
CannotWriteCfg = Не удаётся записать данные в файл конфигурации
CannotReadCfg = Не удаётся прочитать данные из файла конфигурации
PreloaderTitle = Battle Brothers RU Löncher — Загрузка...
MainFormTitle = Battle Brothers RU Löncher
SettingsTitle = Настройки
SettingsGamePath = Путь к папке с игрой
SettingsGogGalaxy = Запускать игру через GOG Galaxy
SettingsOpeningPanel = Панель, показываемая при открытии лаунчера
SettingsOpeningPanelChangelog = История изменений
SettingsOpeningPanelStatus = Результаты проверки
SettingsOpeningPanelLinks = Полезные ссылки
SettingsOpeningPanelMods = Список модов
SettingsCloseOnLaunch = Закрывать лаунчер при запуске игры
SettingsCreateShortcut = Разместить ярлык на рабочем столе
SettingsOfflineMode = Запускать в оффлайн-режиме
SettingsOfflineModeTooltip = Запускать ли лаунчер сразу в оффлайн-режиме.[n]Не отражает состояние лаунчера, если он запущен в оффлайн-режиме из-за технических неполадок.
SettingsMakeBackup = Сделать резервную копию
SettingsMakeBackupInfo = Резервная копия всех файлов игры, с которыми работает лаунчер, будет создана в папке {0}[n]Продолжить?
SettingsMakeBackupDone = Резервная копия файлов успешно создана в {0}
OfflineModeSet = Оффлайн-режим включён. Перезагрузить лаунчер, чтобы изменения вступили в силу?
OnlineModeSet = Оффлайн-режим выключен. Перезагрузить лаунчер, чтобы изменения вступили в силу?
GamePathSelectionTitle = Укажите папку с игрой
EnterThePath = Укажите путь к папке, в которую установлена игра
NoExeInPath = В указанной папке отсутствует исполняемый файл игры!
OldGameVersion = Ваша версия игры ({0}) не поддерживается!
GogGalaxyDetected = Мы заметили, что у вас установлен GOG Galaxy. Должны ли мы запускать игру через него?
UpdatingLoncher = Обновляем лаунчер Battle Brothers RU...
UpdDownloading = Обновляем лаунчер - Скачиваем {0} ...
PreparingChangelog = Читаем новости...
PreparingToLaunch = Проверяем файлы и запускаем лаунчер...
CannotGetLocaleFile = Не удаётся загрузить файлы локализации
CannotGetImages = Не удаётся загрузить картинки
CannotSetBackground = Не удаётся установить изображение в качестве фона
CannotGetFonts = Не удаётся загрузить шрифты
CannotCheckFiles = Не удаётся проверить файлы
MultipleFileBlocksForSingleGameVersion = В настройках повторяется блок файлов для версии "{0}"
CannotParseConfig = Не удаётся прочитать файл конфигурации
CannotLoadIcon = Не удаётся загрузить иконку
CannotParseSettings = Не удаётся прочитать настройки лаунчера
CannotUpdateLoncher = Не удаётся обновить лаунчер
LoncherOutOfDate1 = Лаунчер устарел.[n][n]И, судя по всему, админ не положил ссылку на новую версию,[n]поэтому мы просто не дадим запустить лаунчер.
LoncherOutOfDate2 = Обновлённый лаунчер имеет проверочную сумму своего предка и не соответствует требуемой проверочной сумме.[n][n]Админ либо не обновил проверочную сумму, либо не обновил ссылку на лаунчер.[n]Запускаться мы при таком раскладе не будем из соображений безопасности.
LoncherOutOfDate3 = Версии лаунчера указывают друг на друга в качестве более новой.[n]Прекращаем работу во избежание зацикливания обновления.
WebClientError = Не удаётся получить настройки для лаунчера.[n]Вероятно, у вас нет подключения к интернету.
WebClientErrorOffline = Не удаётся получить настройки для лаунчера.[n]Вероятно, у вас нет подключения к интернету.[n][n]Повторить попытку подключения, или попробовать запустить лаунчер в оффлайн-режиме?
FileCheckNoFilePath = Не указан путь или имя файла.[n]Свяжитесь с человеком, настраивавшим лаунчер (там, откуда скачали лаунчер)." }
FileCheckInvalidFilePath = Указан некорректный или абсолютный путь к файлу.[n]Абсолютные пути запрещены из соображений безопасности.[n]Свяжитесь с человеком, настраивавшим лаунчер (там, откуда скачали лаунчер).[n][n]Проблемный путь:[n]{0}
`;
(function() {
	var locdata = _LOCALE.replace('\r', '').split('\n')
	_LOCALE = {}
	for (var i = 0; i < locdata.length; i++) {
		var di = locdata[i]
		var eqidx = di.indexOf('=')
		if (eqidx > -1) {
			_LOCALE[di.substring(0, eqidx).trim()] = di.substring(eqidx + 1).trim()
		}
	}
})();