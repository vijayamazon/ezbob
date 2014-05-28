var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.UploadHmrcView = Backbone.Marionette.ItemView.extend({
	template: '#hmrc-upload-template',

	initialize: function(options) {
		this.uploadUi = new EzBob.HmrcUploadUi({
			el: '.hmrc-upload-ui',
			formID: 'hmrcUploadZone',
			uploadUrl: '/Underwriter/UploadHmrc/SaveFile',
			loadPeriodsUrl: '/Underwriter/UploadHmrc/LoadPeriods?customerId=' + options.customerId,
			headers: { 'ezbob-underwriter-customer-id': options.customerId, },
			classes: {
				backBtn: 'btn btn-primary back',
				doneBtn: 'btn btn-primary uploadHmrc',
			},
			clickBack: function() { EzBob.App.vent.trigger('ct:marketplaces.uploadHmrcBack'); },
			clickDone: function() { EzBob.App.vent.trigger('ct:marketplaces.history', null); },
		});

		console.log('upload ui is', this.uploadUi);
	}, // initialize

	onRender: function() {
		this.uploadUi.render();
		return this;
	}, // onRender
}); // EzBob.Underwriter.UploadHmrcView
