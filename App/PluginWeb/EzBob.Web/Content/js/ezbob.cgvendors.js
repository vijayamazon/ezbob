var EzBob = EzBob || {};

(function() {
	var oAll = null;
	var oPure = null;

	EzBob.CgVendors = {
		init: function(oAllVendors) {
			if (oAll)
				return;

			oAll = oAllVendors;

			oPure = {};

			for (var i in oAll)
				if (i != 'HMRC')
					oPure[i] = oAll[i];
		}, // init

		all: function() {
			window.EzBob_CgVendors_init();
			return oAll;
		}, // all

		pure: function() {
			window.EzBob_CgVendors_init();
			return oPure;
		}, // pure
	}; // EzBob.CgVendors
})();
