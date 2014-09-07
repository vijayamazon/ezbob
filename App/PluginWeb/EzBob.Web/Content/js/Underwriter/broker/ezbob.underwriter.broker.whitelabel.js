var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.BrokerWhiteLabelModel = Backbone.Model.extend({
	idAttribute: "Id",
	url: function () {
		return window.gRootPath + "Underwriter/Brokers/LoadWhiteLabel/?nBrokerID=" + this.get("brokerID");
	},
});

EzBob.Underwriter.BrokerWhiteLabelView = EzBob.ItemView.extend({
	template: "#broker-white-label-template",
	initialize: function (options) {
		this.brokerId = options.brokerId;
		this.model.on("change reset", this.render, this);
		this.model.fetch();
		
	},

	events: {
		'focusin input': 'doNothing',
		'focus input': 'doNothing',
		'focusout input': 'doNothing',
		'blur input': 'doNothing',
		
		'click #saveWhiteLabel': 'saveWhiteLabel'
	},
	ui: {
		LogoUploadZone: '#logoUploadZone',
		Form: '#whitelabel-form',
		Logo: '#Logo',
		LogoImageType: '#LogoImageType',
	},
	
	serializeData: function () {
		return {
			whiteLabel: this.model.toJSON(),
			brokerId: this.brokerId
		};
	},

	onRender: function () {
		if (this.ui.LogoUploadZone.length != 0) {
			this.initDropzone();
		}
	},
	
	saveWhiteLabel: function(e) {
		e.preventDefault();
		var data = this.ui.Form.serializeArray();
		var xhr = $.post(this.ui.Form.attr('action'), data);
		var self = this;
		xhr.done(function() {
			self.model.fetch();
		});
	},
	
	doNothing: function () {
		return false;
	},
	
	initDropzone: function () {
		this.clearDropzone();
		var self = this;

		this.Dropzone = new Dropzone(this.ui.LogoUploadZone[0], {
			parallelUploads: 1,
			uploadMultiple: false,
			acceptedFiles: "image/*",
			autoProcessQueue: true,
			headers: { 'ezbob-underwriter-customer-id': self.customerId, },

			init: function () {
				var oDropzone = this;

				oDropzone.on('success', function (oFile, oResponse) {
					if (oResponse.error) {
						alertify.error(oResponse.error);
					}
					else {
						self.ui.Logo.val(oResponse.Logo);
						self.ui.LogoImageType.val(oResponse.LogoType);
					} // if
				}); // on success

				oDropzone.on('error', function (oFile, sErrorMsg, oXhr) {
					if (oXhr && (oXhr.status === 404)) {
						alertify.error('Error uploading ' + oFile.name + ': file is too large. ');
					} else {
						alertify.error('Error uploading ' + oFile.name + ': ' + sErrorMsg);
					}
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
