var EzBob = EzBob || {};

(function() {
	EzBob.Csrf = {
		fieldSelector: "input[name='__RequestVerificationToken']",

		headerName: 'X-Request-Verification-Token',

		init: function() {
			var self = this;

			$.ajaxPrefilter(function(options, originalOptions, jqXHR) {
				var verificationToken = self.token();

				if (verificationToken)
					jqXHR.setRequestHeader(self.headerName, verificationToken);
			});
		}, // init

		token: function() {
			return $(this.fieldSelector).val();
		}, // token

		updateToken: function(sNewFieldHtml) {
			$(this.fieldSelector).val($(sNewFieldHtml).val());
		}, // updateToken
	}; // EzBob.Csrf

	EzBob.Csrf.init();
})();
