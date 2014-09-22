var EzBob = EzBob || {};

EzBob.CompanyFilesAccountModel = Backbone.Model.extend({
	urlRoot: '' + window.gRootPath + 'Customer/CompanyFilesMarketPlaces/Accounts',
}); // EzBob.CompanyFilesAccountModel

EzBob.CompanyFilesAccounts = Backbone.Collection.extend({
	model: EzBob.CompanyFilesAccountModel,
	url: '' + window.gRootPath + 'Customer/CompanyFilesMarketPlaces/Accounts',
}); // EzBob.CompanyFilesAccounts

EzBob.CompanyFilesAccountInfoView = Backbone.Marionette.ItemView.extend({
	initialize: function() {
		this.accountType = 'CompanyFiles';
		this.template = '#' + this.accountType + 'AccountInfoTemplate';
		this.Dropzone = null;
	}, // initialize

	events: {
		'click a.back': 'back',
		'click a.connect-account': 'connect'
	}, // events

	ui: {
		companyFilesUploadZone: '#companyFilesUploadZone',
		uploadButton: '.connect-account'
	}, // ui

	onRender: function() {
	    EzBob.UiAction.registerView(this);
		this.initDropzone();

		return this;
	}, // render

	back: function() {
		this.trigger('back');
		return false;
	}, // back

	getDocumentTitle: function() {
		EzBob.App.trigger('clear');
		return 'Upload Company Files';
	}, // getDocumentTitle

	initDropzone: function() {
		this.clearDropzone();

		Dropzone.options.customerFilesUploader = false;

		var self = this;

		this.Dropzone = new Dropzone(this.ui.companyFilesUploadZone[0], {
			maxFilesize: EzBob.Config.CompanyFilesMaxFileSize,
			acceptedFiles: EzBob.Config.CompanyFilesAcceptedFiles,

			maxFiles: 10,

			success: function(oFile, oResponse) {
				var enabled;

				if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
					enabled = this.getAcceptedFiles() !== 0;
					self.ui.uploadButton.toggleClass('disabled', !enabled);
				} // if

				if (oResponse.success)
					EzBob.App.trigger('info', 'Upload successful: ' + oFile.name);
				else if (oResponse.error)
					EzBob.App.trigger('error', oResponse.error);
			}, // on success

			error: function(oFile, sErrorMsg, oXhr) {
				if (oXhr && (oXhr.status === 404)) {
					EzBob.App.trigger('error',
						'Error uploading ' + oFile.name + ': file is too large. ' +
						'Please contact customercare@ezbob.com'
					);
				}
				else
					EzBob.App.trigger('error', 'Error uploading ' + oFile.name);

				this.removeFile(oFile);
			}, // on error

			maxfilesexceeded: function(o) {
				EzBob.App.trigger('error', 'You can upload up to 10 files');
			}, // on maxfilesexceeded
		}); // new Dropzone
	}, // initDropzone

	clearDropzone: function() {
		if (this.Dropzone) {
			this.Dropzone.destroy();
			this.Dropzone = null;
		} // if
	}, // clearDropzone

	connect: function() {
		if (this.ui.uploadButton.hasClass('disabled'))
			return false;

		BlockUi('on');

		var xhr = $.post(window.gRootPath + 'CompanyFilesMarketPlaces/Connect', {
			customerId: this.customerId
		});

		xhr.done(function(res) {
			if (res.error !== void 0)
				return EzBob.App.trigger('error', 'Failed to upload company files');
			else
				return EzBob.App.trigger('info', 'Company files uploaded successfully');
		});

		xhr.always(function() {
			return BlockUi('off');
		});

		this.trigger('completed');
		this.trigger('back');

		return false;
	}, // connect
}); // EzBob.CompanyFilesAccountInfoView
