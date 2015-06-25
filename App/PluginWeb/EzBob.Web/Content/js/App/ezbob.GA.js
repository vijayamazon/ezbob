var EzBob = EzBob || {};

EzBob.GA = (function() {
	function GA() { }

	GA.prototype.trackPage = function(url, title, options) {
		if (dataLayer) {
			var gtmPushRequest = {
				'event': 'VirtualPageview',
				'virtualPageURL': url,
				'virtualPageTitle': title,
				'Device': navigator.userAgent
			};

			if (options) {
				for (var i in options) {
					if (options.hasOwnProperty(i)) {
						gtmPushRequest[i] = options[i];
					}
				}
			}

			dataLayer.push(gtmPushRequest);
		}

		//if ((typeof _gaq !== "undefined" && _gaq !== null))
		//		_gaq.push(['_trackPageview', url]);
		//	else if ((typeof ga !== "undefined" && ga !== null)) {
		//		ga('create', 'UA-32583191-1', 'auto');
		//		ga('send', 'pageview', url);
		//	} else
		//		console.log('Track PageView: %s', url);
		//}
	};

	return GA;
})();
