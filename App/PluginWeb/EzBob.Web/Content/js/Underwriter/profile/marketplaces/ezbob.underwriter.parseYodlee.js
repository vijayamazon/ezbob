var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ParseYodleeView = Backbone.Marionette.ItemView.extend({
	template: '#parse-yodlee-template',

	initialize: function(options) {
		this.customerId = options.customerId;
		this.model.on("reset change sync", this.render, this);
	}, // initialize
	events: {
		"click .back": "back",
		"click .parseYodlee": "parseYodlee",
	},
	serializeData: function () {
		var companyFiles = _.find(this.model.models, function (model) {
			return model.get('CompanyFiles') != null;
		});
		return {
			customerId: this.customerId,
			files: companyFiles ? companyFiles.get('CompanyFiles').Files : []
		};
	},
	onRender: function () {
		this.initDropzone();
		return this;
	}, // onRender
	
	parseYodlee: function () {
		var fileId = this.$el.find("[name='YodleeBankFile']:checked").val();
		if (fileId) {
			var xhr = $.post(window.gRootPath + "Underwriter/MarketPlaces/ParseYodlee", { fileId: fileId, customerId: this.customerId });
			xhr.done(function (res) {
			    if (res.error) {
			        EzBob.ShowMessage("Parsing of file failed: " + res.error, "Parsing failed");
			        return;
			    }
				EzBob.ShowMessageTimeout("Parsing of file began, refresh in a while", "Parsing began", 3);
			});
		}
	},
	
	back: function () {
		EzBob.App.vent.trigger('ct:marketplaces.parseYodleeBack');
	},
	
	initDropzone: function () {
		this.clearDropzone();
		var self = this;

		this.Dropzone = new Dropzone(this.$el.find('#bankFilesUploadZone')[0], {
			url: window.gRootPath + "Underwriter/MarketPlaces/UploadFile",
			parallelUploads: 10,
			uploadMultiple: true,
			maxFilesize: EzBob.Config.CompanyFilesMaxFileSize,
			acceptedFiles: EzBob.Config.CompanyFilesAcceptedFiles,
			autoProcessQueue: true,
			headers: { 'ezbob-underwriter-customer-id': self.customerId, },

			init: function () {
				var oDropzone = this;

				oDropzone.on('success', function (oFile, oResponse) {
					EzBob.ServerLog.debug(
						'Upload',
						(oResponse.success ? '' : 'NOT'),
						'succeeded. File name is', oFile.name,
						' file size is', oFile.size,
						' file type is', oFile.type,
						' Error message:', oResponse.error
					);

					if (oResponse.success) {
					    alertify.success('Upload successful: ' + oFile.name);
					    return;
					}
					else if (oResponse.error) {
					    alertify.error(oResponse.error);
					}
					else {
					    alertify.error('Failed to upload ' + oFile.name);
					} // if
				}); // on success

				oDropzone.on('error', function (oFile, sErrorMsg, oXhr) {
					EzBob.ServerLog.warn(
						'Upload error.',
						' File name is', oFile.name,
						' file size is', oFile.size,
						' file type is', oFile.type,
						' Error message:', sErrorMsg
					);

					if (oXhr && (oXhr.status === 404)) {
					    alertify.error('Error uploading ' + oFile.name + ': file is too large. ' +
								'Please contact customercare@ezbob.com'
						);
					} else {
					    alertify.error('Error uploading ' + oFile.name + ': ' + sErrorMsg);
					}
				}); // always

				oDropzone.on('complete', function (oFile) {
					oDropzone.removeFile(oFile);
					EzBob.App.vent.trigger('ct:marketplaces.addedFile');
				}); // always
			}, // init
		}); // create a dropzone
	}, // initDropzone
	
	clearDropzone: function () {
		if (this.Dropzone) {
			this.Dropzone.destroy();
			this.Dropzone = null;
		} // if
	}, // clearDropzone
	
}); 
