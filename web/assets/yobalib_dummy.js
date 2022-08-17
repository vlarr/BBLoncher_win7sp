var YL = new (function () {

	this.GetLoc = function (key) {
		return key
	}
	this.GetLocs = function (keys) {
		var res = {}
		for (var i = 0; i < keys.length; i++) {
			res[keys[i]] = keys[i]
		}
		return res
	}

	var precachedLocale = {}
	var _self = this
	var viewUpdateHandlers = {}

	function bindViewUpdateHandler(viewId, handler) {
		if (!viewUpdateHandlers.hasOwnProperty(viewId)) {
			viewUpdateHandlers[viewId] = []
		}
		viewUpdateHandlers[viewId].push(handler)
	}
	this.__fireViewUpdate = function (viewId, data) {
		if (viewUpdateHandlers.hasOwnProperty(viewId)) {
			var hs = viewUpdateHandlers[viewId]
			for (var i = 0; i < hs.length; hs[i++](data));
		}
	}
	var viewIds = ['Status', 'Mods', 'FAQ', 'Changelog', 'Links']
	for (var i = 0; i < viewIds.length; i++) {
		(function (viewId) {
			_self['Bind' + viewId + 'ViewUpdateHandler'] = function (handler) {
				bindViewUpdateHandler(viewId, handler)
			}
		})(viewIds[i]);
	}

	var __buffunc;
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
				Warn('Вызов отсутствующей буферной функции "' + funcName + '". Сообщите об этом происшествии авторам лаунчера.')
			}
		}
		else {
			Warn('Вызов буферной функции при пустом буфере. Сообщите об этом происшествии авторам лаунчера.')
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

})();