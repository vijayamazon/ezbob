var EzBob = EzBob || {};

EzBob.ConsentAgreementModel = Backbone.Model.extend({
	defaults: {
		id: 0,
		firstName: '',
		middleInitial: '',
		surname: '',
		date: moment.utc().toDate()
	} // defaults
}); // EzBob.ConsentAgreementModel

EzBob.ConsentAgreement = Backbone.Marionette.ItemView.extend({
	template: '#consent-agreement-template',

	events: {
		'click .print': 'onPrint',
		'click .download': 'onDownload',
		'click .close-terms': 'closeSelf',
	}, // events

	jqoptions: function () {
		return {
			autoOpen: true,
			title: 'Terms and conditions',
			modal: true,
			resizable: true,
			width: 600,
			height: 'auto',
			closeOnEscape: true,
		};
	}, // jqoptions

	closeSelf: function() {
		this.trigger('close');
	}, // closeSelf

	onRender: function(){
		EzBob.UiAction.registerView(this);
	}, // onRender

	onDownload: function() {
		location.href =
			window.gRootPath + 'Customer/Consent/Download?id=' + this.model.get('id') +
			'&firstName=' + this.model.get('firstName') +
			'&middleInitial=' + this.model.get('middleInitial') +
			'&surname=' + this.model.get('surname');
	}, // onDownload

	onPrint: function() {
		printElement('consent-conent');
	}, // onPrint
}); // EzBob.ConsentAgreement

