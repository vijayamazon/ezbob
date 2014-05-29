var EzBob = EzBob || {};

EzBob.HMRCAccountInfoView = Backbone.Marionette.ItemView.extend({
	initialize: function(options) {
		this.template = '#HMRCAccountInfoTemplate';
		this.activeForm = null;
		this.Dropzone = null;
	}, // initialize

	events: {
		'change input': 'inputChanged',
		'keyup input': 'inputChanged',
		'click a.hmrcBack': 'back',
		'click #uploadButton': 'uploadFiles',
		'click a.linkAccountBack': 'linkAccountBack',
		'click a.uploadFilesBack': 'uploadFilesBack',
		'click a.connect-account': 'connect',
		'click a.connect-account-help': 'connect',
		'click #linkHelpButton': 'getLinkHelp',
		'click #uploadHelpButton': 'getUploadHelp',
		'click #linkButton': 'linkAccount',
		'click a.newVatFilesUploadButton': 'doUploadFiles',
		'click #uploadAndLinkHelpButton': 'getUploadAndLinkHelp',
		'click #uploadAndLinkInfoButton': 'getUploadAndLinkHelp',
		'click #linkInfoButton': 'getLinkHelp',
		'click #uploadInfoButton': 'getUploadHelp'
	}, // events

	clearDropzone: function() {
		if (this.Dropzone) {
			this.Dropzone.destroy();
			this.Dropzone = null;
		} // if
	}, // clearDropzone

	initDropzone: function() {
		this.clearDropzone();

		Dropzone.options.hmrcAccountUpload = false;

		var self = this;

		this.Dropzone = new Dropzone(this.$el.find('#hmrcAccountUpload').addClass('dropzone dz-clickable')[0], {
			parallelUploads: 1,
			uploadMultiple: true,
			acceptedFiles: 'application/pdf',
			autoProcessQueue: true,
			maxFilesize: 10,
			init: function() {
				var oDropzone = this;

				oDropzone.on('success', function(oFile, oResponse) {
					console.log('Upload', (oResponse.success ? '' : 'NOT'), 'succeeded:', oFile, oResponse);

					if (oResponse.success) {
						self.reloadFileList();
						EzBob.App.trigger('info', 'Upload successful: ' + oFile.name);
					}
					else if (oResponse.error)
						EzBob.App.trigger('error', oResponse.error);
					else
						EzBob.App.trigger('error', 'Failed to upload ' + oFile.name);
				}); // on success

				oDropzone.on('error', function(oFile, sErrorMsg, oXhr) {
					console.log('Upload error:', oFile, sErrorMsg, oXhr);
					EzBob.App.trigger('error', 'Error uploading ' + oFile.name + ': ' + sErrorMsg);
				}); // always

				oDropzone.on('complete', function(oFile) {
					oDropzone.removeFile(oFile);
				}); // always
			}, // init
		});
	}, // initDropzone

	reloadFileList: function() {
		// TODO
		this.$el.find('a.newVatFilesUploadButton').toggleClass('disabled'); // , !enabled);
	}, // reloadFileList

	onRender: function() {
		this.clearDropzone();

		this.initDropzone();

		this.reloadFileList();

		var btn = this.$el.find('.hmrcAnimatedButton');

		btn.hoverIntent(
			function(evt) { $('.onhover', this).animate({ top:      0, opacity: 1 }); },
			function(evt) { $('.onhover', this).animate({ top: '80px', opacity: 0 }); }
		);

		return this;
	}, // onRender

	inputChanged: function() {
		if (this.activeForm === null) {
			this.activeForm = this.$el.find('#hmrcLinkAccountForm');
			this.validator = EzBob.validateHmrcLinkForm(this.activeForm);
		} // if

		var enabled = EzBob.Validation.checkForm(this.validator);

		this.$el.find('a.connect-account').toggleClass('disabled', !enabled);

		this.$el.find('a.connect-account-help').toggleClass('disabled', !enabled);
	}, // inputChanged

	linkAccount: function() {
		this.activeForm = this.$el.find('#hmrcLinkAccountForm');
		this.validator = EzBob.validateHmrcLinkForm(this.activeForm);
		this.$el.find('#linkAccountDiv').show();
		this.$el.find('#initialDiv').hide();
	}, // linkAccount

	uploadFiles: function() {
		this.$el.find('#uploadFilesDiv').show();
		this.$el.find('#initialDiv').hide();
	}, // uploadFiles

	doUploadFiles: function() {
		if (this.$el.find('.newVatFilesUploadButton').hasClass('disabled'))
			return false;

		this.trigger('completed');
		this.trigger('back');
		this.$el.find('#uploadFilesDiv').hide();
		this.$el.find('#initialDiv').show();

		return false;
	}, // doUploadFiles

	back: function() {
		this.trigger('back');
		return false;
	}, // back

	linkAccountBack: function() {
		this.$el.find('#linkAccountDiv').hide();
		this.$el.find('#initialDiv').show();
		return false;
	}, // linkAccountBack

	uploadFilesBack: function() {
		this.$el.find('#uploadFilesDiv').hide();
		this.$el.find('#initialDiv').show();
		return false;
	}, // uploadFilesBack

	getDocumentTitle: function() {
		EzBob.App.trigger('clear');
		return 'Link HMRC Account';
	}, // getDocumentTitle

	connect: function() {
		if (this.activeForm === null) {
			this.activeForm = this.$el.find('#hmrcLinkAccountForm');
			this.validator = EzBob.validateHmrcLinkForm(this.activeForm);
		} // if

		if (!EzBob.Validation.checkForm(this.validator)) {
			this.validator.form();
			return false;
		} // if

		if (this.$el.find('a.connect-account').hasClass('disabled'))
			return false;

		var accountModel = this.buildModel();

		if (!accountModel) {
			EzBob.App.trigger('error', 'HMRC Account Data Validation Error');
			return false;
		}

		var acc = new EzBob.CGAccountModel(accountModel);
		var xhr = acc.save();

		if (!xhr) {
			EzBob.App.trigger('error', 'HMRC Account Saving Error');
			return false;
		} // if

		var _this = this;

		BlockUi('on');

		xhr.always(function() { return BlockUi('off'); });

		xhr.fail(function(jqXHR, textStatus, errorThrown) {
			return EzBob.App.trigger('error', 'Failed to Save HMRC Account');
		});

		xhr.done(function(res) {
			if (res.error) {
				EzBob.App.trigger('error', res.error);
				return false;
			} // if

			try {
				_this.model.add(acc);
			}
			catch (_error) {
				// Silently ignore.
			} // try

			EzBob.App.trigger('info', 'HMRC Account Added Successfully');

			_this.$el.find('#hmrc_user_id').val("");
			_this.$el.find('#hmrc_password').val("");
			_this.$el.find('#linkAccountDiv').hide();

			_this.$el.find('#initialDiv').show();

			_this.activeForm = _this.$el.find('#hmrcLinkAccountForm');
			_this.validator = EzBob.validateHmrcLinkForm(_this.activeForm);

			_this.inputChanged();

			_this.trigger('completed');
			_this.trigger('back');

			return false;
		});

		return false;
	}, // connect

	buildModel: function() {
		var accountModel;
		accountModel = $.parseJSON($('div#cg-account-model-template').text());
		accountModel.accountTypeName = 'HMRC';
		accountModel.login = this.$el.find('#hmrc_user_id').val();
		accountModel.name = this.$el.find('#hmrc_user_id').val();
		accountModel.password = this.$el.find('#hmrc_password').val();
		delete accountModel.id;
		return accountModel;
	}, // buildModel

	getLinkHelp: function() {
		var oDialog = $('#hmrcLinkHelpPopup');

		if (oDialog.length > 0) {
			$.colorbox({
				inline: true,
				open: true,
				href: oDialog,
				width: '35%'
			});
		} // if
	}, // getLinkHelp

	getUploadHelp: function() {
		var oDialog = $('#hmrcUploadHelpPopup');

		if (oDialog.length > 0) {
			$.colorbox({
				inline: true,
				open: true,
				href: oDialog,
				width: '35%'
			});
		} // if
	}, // getUploadHelp

	getUploadAndLinkHelp: function() {
		var oDialog = $('#hmrcUploadAndLinkHelpPopup');

		if (oDialog.length > 0) {
			$.colorbox({
				inline: true,
				open: true,
				href: oDialog,
				width: '65%'
			});
		} // if
	}, // getUploadAndLinkHelp
}); // EzBob.HMRCAccountInfoView
