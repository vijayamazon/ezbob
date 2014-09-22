(function() {
	if (!(window.console && window.console.log))
		return;

	var _log = function() {
		return console.log.apply(console, makeArray(arguments));
	};

	var makeArray = function(arrayLikeThing) {
		return Array.prototype.slice.call(arrayLikeThing);
	};

	var log = function() {
		var args = [];

		makeArray(arguments).forEach(function(arg) {
			if (typeof arg === 'string')
				args = args.concat(stringToArgs(arg));
			else
				args.push(arg);
		});

		return _log.apply(window, args);
	};

	var formats = [
		{
			regex: /\*([^\*)]+)\*/,
			replacer: function(m, p1) {
				return "%c" + p1 + "%c";
			},
			styles: function() {
				return ['font-style: italic', ''];
			}
		}, {
			regex: /\_([^\_)]+)\_/,
			replacer: function(m, p1) {
				return "%c" + p1 + "%c";
			},
			styles: function() {
				return ['font-weight: bold', ''];
			}
		}, {
			regex: /\`([^\`)]+)\`/,
			replacer: function(m, p1) {
				return "%c" + p1 + "%c";
			},
			styles: function() {
				return ['background: rgb(255, 255, 219); padding: 1px 5px; border: 1px solid rgba(0, 0, 0, 0.1)', ''];
			}
		}, {
			regex: /\[c\=\"([^\")]+)\"\]([^\[)]+)\[c\]/,
			replacer: function(m, p1, p2) {
				return "%c" + p2 + "%c";
			},
			styles: function(match) {
				return [match[1], ''];
			}
		}
	];

	var hasMatches = function(str) {
		var _hasMatches = false;

		formats.forEach(function(format) {
			if (format.regex.test(str))
				_hasMatches = true;
		});

		return _hasMatches;
	};

	var getOrderedMatches = function(str) {
		var matches = [];

		formats.forEach(function(format) {
			var match = str.match(format.regex);

			if (match) {
				matches.push({
					format: format,
					match: match
				});
			}
		});

		return matches.sort(function(a, b) {
			return a.match.index - b.match.index;
		});
	};

	var stringToArgs = function(str) {
		var styles = [];

		while (hasMatches(str)) {
			var matches = getOrderedMatches(str);
			var firstMatch = matches[0];
			str = str.replace(firstMatch.format.regex, firstMatch.format.replacer);
			styles = styles.concat(firstMatch.format.styles(firstMatch.match));
		} // while

		return [str].concat(styles);
	};

	var isSafari = function() {
		return (/Safari/).test(navigator.userAgent) && /Apple Computer/.test(navigator.vendor);
	};

	var isIE = function() {
		return (/MSIE/).test(navigator.userAgent);
	};

	if (isSafari() || isIE())
		window.log = _log;
	else
		window.log = log;

	window.log.l = _log;
})();
