var EzBob = EzBob || {};

(function() {
	EzBob.EditExperianDirectorData = function() {};

	EzBob.EditExperianDirectorData.prototype.init = function(options) {
		var defaults = {
			directorID: 0,
			email: '',
			mobilePhone: '',
			line1: '',
			line2: '',
			line3: '',
			town: '',
			county: '',
			postcode: '',

			saveUrl: '',

			editBtn: null,
			saveBtn: null,
			cancelBtn: null,

			emailCell: null,
			mobilePhoneCell: null,
			addressCell: null,
		};

		this.data = _.extend({}, defaults, options);
	}; // init
})();
