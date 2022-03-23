
$.fn.onActivate = function(fn) {
	return this.click(fn).keyup(function(e) {
		if (e.key == "Enter" || e.key == "Spacebar" || e.key == " ") {
			fn.apply(this)
			
		}
	}).keydown(function(e) {
		if (e.keyCode === 32 || e.keyCode === 13) {
			e.preventDefault()
		}
	})
}
$.fn.onEnter = function(fn) {
	return this.keyup(function(e) {
		if (e.key == "Enter") {
			fn.apply(this)
		}
	})
}
$.fn.optClass = function (flag, cls) {
	return this[flag ? 'addClass' : 'removeClass'](cls)
};

document.oncontextmenu = function(e){
	return false
}

var progressBar = {
	SetValue: function(value) {
		value = Math.floor(this.valSeg * value)
		if (this._IsInit) {
			if (value < 1) {
				this.bar$.hide()
			}
			else {
				this.bar$.show()
				if (value < 8) {
					this.mid.style.width = '0'
					this.start.style.width = '3px'
					this.end.style.width = '4px'
				}
				else if (value < 17) {
					var ev = value % 2
					var half = (value - ev) / 2
					this.mid.style.width = '0'
					this.start.style.width = '' + (half + ev) + 'px'
					this.end.style.width = '' + half + 'px'
				}
				else {
					this.mid.style.width = '' + (value - 16) + 'px'
					this.start.style.width = '8px'
					this.end.style.width = '8px'
				}
			}
		}
		else {
			this.InitVal = value
		}
	}
	, Init: function(max) {
		this.bar$ = $('.progress .bar')
		this.valSeg = this.bar$.width() / max
		this.mid = $('.progress .bar .mid')[0]
		this.start = $('.progress .bar .start')[0]
		this.end = $('.progress .bar .end')[0]

		this._IsInit = true
		this.SetValue(this.InitVal)
	}
	, _IsInit: false
	, InitVal: 0
}
var progressTitle = {
	SetValue: function(value) {
		if (this._IsInit) {
			this.caption$.text(value)
		}
		else {
			this.InitVal = value
		}
	}
	, Init: function() {
		this.caption$ = $('.progress-status .caption')
		this._IsInit = true
		this.SetValue(this.InitVal)
	}
	, _IsInit: false
	, InitVal: ""
}
var launchBtn = {
	Update: function(isReady, isEnabled) {
		this.IsReady = isReady
		this.IsEnabled = isEnabled
		if (this._IsInit) {
			this.element$.optClass(!isEnabled, 'disabled')
			this.caption$.text(mainLocale[isReady ? 'LaunchBtn' : 'UpdateBtn'])
		}
	}
	, Init: function() {
		var jq = this.element$ = $('#LaunchBtn').onActivate(function() {
			if (!this.classList.contains('disabled')) {
				YL.LaunchGame()
			}
		})
		this.caption$ = jq.find('.caption')
		this._IsInit = true
		this.Update(this.IsReady, this.IsEnabled)
	}
	, _IsInit: false
	, IsEnabled: false
	, IsReady: true
}

function StyledComboBox(selectedIdx, variants) {
	var containerEl = $('<div class="comboBox">');
	var valueDisplay = $('<div class="valueDisplay">').appendTo(containerEl);
	var dropdownContainer = $('<div class="dropdownContainer">').appendTo(containerEl);
	var disabled = false;
	var _selfCB = this
	var lastddv = false
	_selfCB.Container = containerEl
	
	for (var i = 0; i < variants.length; i++) {
		var variant = variants[i];
		var dropdownVariant = $('<div class="dropdownVariant">').appendTo(dropdownContainer).text(variant.Text);
		dropdownVariant[0].__variant = variant
		dropdownVariant[0].__select = function() {
			if (!disabled) {
				if (_selfCB.Value != this.__variant.Value) {
					valueDisplay.empty().text(this.__variant.Text)
					_selfCB.Value = this.__variant.Value
					if (typeof _selfCB.OnSelect == 'function') {
						_selfCB.OnSelect()
					}
					dropdownContainer[0].deferred(function() {
						updateDDPos()
					})
				}
				lastddv = this
				dropdownContainer.toggle()
			}
		}
		dropdownVariant.onActivate(function() {
			this.__select()
		})
		dropdownVariant.on('keypress', function(e) {
			switch (e.key) {
				case 'ArrowUp':
				case 'Up':
					(this.previousSibling || this.parentNode.lastChild || this).focus()
					break;
				case 'ArrowDown':
				case 'Down':
					(this.nextSibling || this.parentNode.firstChild || this).focus()
					break;
			}
		}).on('keydown', function(e) {
			switch (e.keyCode) {
				case 38:
				case 40:
					e.preventDefault()
					break;
			}
		})
		if (i == selectedIdx) {
			dropdownVariant[0].__select()
		}
	}

	this.SetTabIndex = function(tabidx) {
		valueDisplay.attr('tabindex', tabidx)
	}

	this.Disable = function() {
		containerEl.addClass('disabled')
		disabled = true
	}
	function updateDDPos() {
		var iw = containerEl.innerWidth()
		var style = dropdownContainer[0].style
		style.top = containerEl.innerHeight() + 'px'
		style.minWidth = iw + 'px'
		style.left = (containerEl.outerWidth() - iw) / 2 + 'px'
	}
	dropdownContainer[0].deferred(function() {
		updateDDPos()
	})
	valueDisplay.onActivate(function() {
		if (!disabled) {
			dropdownContainer.toggle()
			dropdownContainer[0].deferred(function() {
				lastddv.focus()
			})
			//$(lastddv).trigger('focus')
		}
	})
}

$(function() {
	YL.UpdateAppControlsSize("130", "26")
	var bg = YL.RetrieveBackground()
	document.body.style.backgroundImage = 'url("' + bg + '")'

	var startViewId = YL.RetrieveStartViewId()
	if (startViewId && startViewId.length > 2) {
		_START_VIEW_ID = startViewId
	}

	$(document.body).keyup(function(e) {
		if (e.key == 'Alt') {
			var target = $(e.target)
			if (target.is('.app-control-btn')) {
				launchBtn.element$.focus()
			}
			else if (target.is('.app-btn')) {
				$('.app-controls .help').focus()
			}
			//else if (target.is('.app-btn')) {
			//	$('.app-controls .help').focus()
			//}
		}
	})

	var doReset = true
	$(document).on('focus', '.app-control-btn, .app-btn, .modControlButton, .valueDisplay, .dropdownVariant, .customLink', function (e) {
		$(e.target).addClass('tabbed')
		doReset = true
	});
	$(document).on('keydown mousedown mouseup', function (e) {
		if (doReset) {
			$('.tabbed').removeClass('tabbed')
			doReset = false
		}
	});

	var progressBarState = YL.GetProgressBarState()
	progressTitle.SetValue(progressBarState.Caption)
	progressBar.Init(progressBarState.MaxValue)
	progressBar.SetValue(progressBarState.Progress)
	progressTitle.Init()
	launchBtn.Init()

	var appBtns = $('.app-buttons')
	var views = new (function () {
		var views_ = []
		var lastActive = false
		this.Reg = function(viewId) {
			views_.push({
				Id: viewId
				, Container: $('#' + btnId + 'View')
			})
		}
		this.Show = function(viewId) {
			if (lastActive != viewId) {
				if (lastActive) {
					$('#' + lastActive + 'Btn').removeClass('active')
					$('#' + lastActive + 'View').removeClass('active').hide() //[0].setAttribute('disabled', '1')
				}
				lastActive = viewId
				$('#' + viewId + 'Btn').addClass('active')
				$('#' + lastActive + 'View').addClass('active').show() //[0].removeAttribute('disabled')
			}
		}
	})
	var appBtnsIds = ['Status', 'Mods', 'Changelog', 'Links', 'FAQ']
	for (var i = 0; i < appBtnsIds.length; i++) {
		var btnId = appBtnsIds[i]
		views.Reg(btnId)
		var btn = $('<div class="app-btn">').appendTo(appBtns)
			.attr('title', mainLocale[btnId + 'Tooltip']).attr('id', btnId + 'Btn').attr('tabindex', i + 3)
			.append(
				$('<div class="caption">').text(mainLocale[btnId + 'Btn'])
			).onActivate(function() {
				views.Show(this.viewId)
			})
		btn[0].viewId = btnId
	}
	$('#SettingsBtn').attr('title', mainLocale['SettingsTooltip']).onActivate(function() {
		YL.AppSettings()
	})
	$('#SettingsBtn .caption').text(mainLocale['SettingsBtn'])


	;(function() {
		// Changelog Init

		var changelogSections = []

		var indesplit = changelograw.split('=====|=|=|=====');
		for (var i = 0; i < indesplit.length; i++) {
			var pair = indesplit[i].split('=====Content=====');
			changelogSections.push({ Title: pair[0], Content: pair[1] });
		}

		var items = []
		var selectedItem = null

		var menu = $('#ChangelogView .kb-side-menu')
		var content = $('#ChangelogView .article-content')

		for (var i = 0; i < changelogSections.length; i++) {
			items.push($('<div class="kb-side-item">').attr('acticleId', i).appendTo(menu).text(changelogSections[i].Title).click(function() {
				selectItem($(this))
			}))
		}

		selectItem(items[0])

		function selectItem(item) {
			if (!item.hasClass('active')) {
				if (selectedItem) {
					selectedItem.removeClass('active')
				}
				content.html(changelogSections[item.attr('acticleId')].Content)
				item.addClass('active')
				selectedItem = item
			}
		}
	})();

	// FAQ Init
	$('#FAQView .article-content').html(faqraw)

	YL.UpdateStatusData()
	YL.UpdateModsData()

	for (var i = 0; i < appBtnsIds.length; i++) {
		(function(btnid) {
			var viewsel = '#' + btnid + 'View'
			var el = $(viewsel + ' .article-content')
			if (el.length) {
				addTouchScroll({
					Element: el[0]
					, DrawArrows: true
					, DrawScrollBar: true
					, InbarArrows: true
					, OnReady: function() {
						if (btnid == _START_VIEW_ID) {
							views.Show(btnid)
						}
						else {
							$(viewsel).hide()
						}
					}
				})
			
			}
		})(appBtnsIds[i]);
	}

	$('.preloader').hide()
});