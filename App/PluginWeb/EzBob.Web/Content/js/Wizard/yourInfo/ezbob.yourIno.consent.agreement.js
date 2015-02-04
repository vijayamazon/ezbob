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
	template: '#consent-agreement-temlate',

	events: {
		'click .print': 'onPrint',
        'click .download': 'onDownload'
	}, // events
	onRender:function(){
		EzBob.UiAction.registerView(this);
	},
	onDownload: function() {
		var id = this.model.get('id');
		var firstName = this.model.get('firstName');
		var middleInitial = this.model.get('middleInitial');
		var surname = this.model.get('surname');
		location.href = window.gRootPath + "Customer/Consent/Download?id=" + id + "&firstName=" + firstName + "&middleInitial=" + middleInitial + "&surname=" + surname;
	}, //onDownload
	onPrint: function() {
		printElement("consent-conent");
	}//onPrint
}); // EzBob.ConsentAgreement

