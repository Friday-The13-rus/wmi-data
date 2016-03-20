"use strict";

onerror = Error;
execScript('Function Message(prompt, title)\r\n'
	+ ' Message = MsgBox(prompt, 16, title)\r\n'
	+ 'End Function', "vbscript");
	
function Error(message, source, lineno) {
	var line = 'File: ' + source + '\r\nLine ' + lineno + '\r\n' +
		'Message:' + message;
	System.Diagnostics.EventLog.writeEntry(line, 1);

	Message(line, 'Error!');
}

if (!Array.prototype.forEach) {
	Array.prototype.forEach = function (fun /*, thisp*/) {
		var len = this.length;
		if (typeof fun != "function")
			throw new TypeError();

		var thisp = arguments[1];
		for (var i = 0; i < len; i++) {
			if (i in this)
				fun.call(thisp, this[i], i, this);
		}
	};
}