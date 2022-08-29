;
$.fn.bbCode = function (text) {
	if (!this.length) {
		return this
	}
	if (typeof text !== 'string') {
		return this.text(text)
	}
	this.contents().remove()

	var bbRoot = this
	var bbCurrentParent = this
	var parseStart = 0
	var cursor = 0
	var bracketEnd = 0
	var tagLen = 0
	var bbTagName = ""
	var classLine = ""
	var eqidx = 0
	var bbvalue = ""

	while (true) {
		cursor = text.indexOf('[', cursor)
		if (cursor < 0) {
			addText(text.substring(parseStart))
			break;
		}
		bracketEnd = text.indexOf(']', cursor)
		if (bracketEnd < 0) {
			addText(text.substring(parseStart))
			break;
		}
		tagLen = bracketEnd - cursor - 1
		if (tagLen == 1) {
			bbTagName = text.charAt(cursor + 1).toUpperCase()
			switch (bbTagName) {
				case 'B':
				case 'I':
				case 'S':
				case 'U':
					putLastText()
					bbCurrentParent = $('<' + bbTagName + '>').appendTo(bbCurrentParent)
					break;
				case 'N':
					putLastText()
					$('<br>').appendTo(bbCurrentParent)
					break;
			}
		}
		else if (tagLen == 2) {
			bbTagName = text.substring(cursor + 1, bracketEnd).toUpperCase()
			switch (bbTagName) {
				case 'UL':
				case 'LI':
					putLastText()
					bbCurrentParent = $('<' + bbTagName + '>').appendTo(bbCurrentParent)
					break;
				case '/B':
				case '/I':
				case '/S':
				case '/U':
					closeTag(bbTagName.charAt(1))
					break;
			}
		}
		else if (tagLen == 3) {
			bbTagName = text.substring(cursor + 1, bracketEnd).toUpperCase()
			switch (bbTagName) {
				case '/UL':
				case '/LI':
					closeTag(bbTagName.substring(1))
					break;
				case '/HL':
					closeTag('SPAN')
					break;
			}
		}
		else {
			classLine = text.substring(cursor + 1, bracketEnd)
			eqidx = classLine.indexOf('=')
			if (eqidx > -1) {
				bbTagName = classLine.substring(0, eqidx).toUpperCase()
				bbvalue = classLine.substring(eqidx + 1)
				switch (bbTagName) {
					case 'COLOR':
						if (/^#[a-fA-F0-9]{6}$/.test(bbvalue)) {
							putLastText()
							bbCurrentParent = $('<span class="bb-coloredText" style="color: ' + bbvalue + '">').appendTo(bbCurrentParent)
						}
						break;
					case 'HL':
						if (/^#[a-fA-F0-9]{6}$/.test(bbvalue)) {
							putLastText()
							bbCurrentParent = $('<span class="bb-highlight" style="background-color: ' + bbvalue + '">').appendTo(bbCurrentParent)
						}
						break;
					case 'URL':
						if (isValidURL(bbvalue)) {
							putLastText()
							bbCurrentParent = $('<a class="bb-url" href="' + bbvalue + '">').appendTo(bbCurrentParent)
						}
						break;
				}
			}
			else {
				bbTagName = classLine.toUpperCase()
				switch (bbTagName) {
					case '/COLOR':
						closeTag('SPAN')
						break;
					case '/URL':
						closeTag('A')
						break;
				}
			}
		}
		cursor = bracketEnd + 1
	}

	function closeTag(tagName) {
		var target = bbCurrentParent[0]
		var root = bbRoot[0]
		while (target != root && target.tagName != tagName) {
			target = target.parentNode
		}
		if (target != root) {
			putLastText()
			bbCurrentParent = $(target.parentNode)
		}
	}
	function addText(val) {
		if (val.length > 3) {
			val = val.split(/<br>|<br\/>|\r\n|\n/)
			bbCurrentParent.append(document.createTextNode(val[0]))
			for (var i = 1; i < val.length; i++) {
				bbCurrentParent.append($('<br>'), document.createTextNode(val[i]))
			}
		}
		else {
			bbCurrentParent.append(document.createTextNode(val))
		}
	}
	function putLastText() {
		var line = text.substring(parseStart, cursor)
		if (line.length) {
			addText(line)
		}
		parseStart = bracketEnd + 1
	}

	return this
};