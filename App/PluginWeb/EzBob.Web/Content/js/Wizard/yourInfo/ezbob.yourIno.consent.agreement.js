var EzBob = EzBob || {};

EzBob.ConsentAgreementModel = Backbone.Model.extend({
	defaults: {
		fullName: 'Jane Doe',
		date: 'sssssss'
	}, // defaults
}); // EzBob.ConsentAgreementModel

EzBob.ConsentAgreement = Backbone.Marionette.ItemView.extend({
	template: '#consent-agreement-temlate',

	events: {
		'click .print': 'onPrint'
	}, // events

	onPrint: function() {
		return printElement('consent');
	}, // onPrint
}); // EzBob.ConsentAgreement

