var EzBob = EzBob || {};

EzBob.GA = (function() {
	function GA() { }

	GA.prototype.trackPage = function(url, title) {
		if (dataLayer) {
			dataLayer.push({
				'event':'VirtualPageview',
				'virtualPageURL':url,
				'virtualPageTitle' : title
			});
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
