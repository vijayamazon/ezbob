var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.UploadHmrcView = Backbone.Marionette.ItemView.extend({
	template: '#hmrc-upload-template',

	initialize: function(options) {
		this.customerId = options.customerId;

		this.uploadUi = new EzBob.HmrcUploadUi({
			el: this.$el,
			headers: { 'ezbob-underwriter-customer-id': this.customerId, },
			formID: 'hmrcUploadZone',
			uploadSuccess: _.bind(this.reloadFileList, this),
		});

		console.log('upload ui is', this.uploadUi);
	}, // initialize

	ui: {
		hmrcUploadZone: '#hmrcUploadZone',
		uploadHmrcButton: '.uploadHmrc',
	}, // ui

	events: {
		'click .uploadHmrc': 'uploadHmrcClicked',
		'click .back': 'backClicked',
	}, // events

	serializeData: function() {
		return { customerId: this.customerId, };
	}, // serializeData

	reloadFileList: function(oFile, oResponse) {
		// TODO
		console.log('reload file list:', oFile, oResponse);
		this.ui.uploadHmrcButton.toggleClass('disabled'); // , !enabled);
	}, // reloadFileList

	onRender: function() {
		this.uploadUi.render();

		this.reloadFileList();

		return this;
	}, // onRender

	uploadHmrcClicked: function(event) {
		event.preventDefault();
		event.stopPropagation();

		if (this.ui.uploadHmrcButton.hasClass('disabled'))
			return false;

		EzBob.App.vent.trigger('ct:marketplaces.history', null);

		return false;
	}, // uploadHmrcClicked

	backClicked: function(event) {
		event.preventDefault();
		event.stopPropagation();

		EzBob.App.vent.trigger('ct:marketplaces.uploadHmrcBack');
	}, // backClicked
}); // EzBob.Underwriter.UploadHmrcView
