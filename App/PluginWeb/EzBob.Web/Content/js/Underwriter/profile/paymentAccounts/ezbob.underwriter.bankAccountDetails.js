var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.BankAccountDetailsView = Backbone.Marionette.ItemView.extend({
	template: "#bank-account-details-template",
	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: "Bank Account Details",
			position: "center",
			draggable: false,
			dialogClass: "bank-account-details-popup",
			width: 500
		};
	}
});
