var EzBob = EzBob || {};

EzBob.HmrcAccountInfoView = Backbone.Marionette.ItemView.extend({
	initialize: function() {
		this.template = '#HMRCAccountInfoTemplate';
		this.activeForm = null;

		this.uploadUi = new EzBob.HmrcUploadUi({
			chartMonths: this.options.chartMonths,
			formID: 'hmrcAccountUpload',
			uploadUrl: '/Customer/Hmrc/SaveFile',
			loadPeriodsUrl: '/Customer/Hmrc/LoadPeriods',
			isUnderwriter: false,
			uiEventControlIDs: {
				form: 'hmrc:dropzone',
				backBtn: 'hmrc:upload_back',
				doneBtn: 'hmrc:do_upload',
			},
			classes: {
			    backBtn: 'button btn-grey back',
				doneBtn: 'button btn-green',
			},
			clickBack: _.bind(this.uploadFilesBack, this),
			clickDone: _.bind(this.doUploadFiles, this),
		});
	}, // initialize

	events: {
		'change input': 'inputChanged',
		'keyup input': 'inputChanged',
		'click a.hmrcBack': 'back',
		'click #uploadButton': 'uploadFiles',
		'click a.linkAccountBack': 'linkAccountBack',
		'click a.connect-account': 'connect',
		'click a.connect-account-help': 'connect',
		'click #linkHelpButton': 'getLinkHelp',
		'click #uploadHelpButton': 'getUploadHelp',
		'click #linkButton': 'linkAccount',
		'click #uploadAndLinkHelpButton': 'getUploadAndLinkHelp',
		'click #uploadAndLinkInfoButton': 'getUploadAndLinkHelp',
		'click #linkInfoButton': 'getLinkHelp',
		'click #uploadInfoButton': 'getUploadHelp',

		'click a.uploadFilesBack': 'uploadFilesBack',
		'click a.newVatFilesUploadButton': 'doUploadFiles',
	}, // events

	onRender: function() {
		this.uploadUi.$el = this.$el.find('.hmrc-upload-ui');
		this.uploadUi.render();

		var btn = this.$el.find('.hmrcAnimatedButton');

		btn.hoverIntent(
			function() { $('.onhover', this).animate({ top:      0, opacity: 1 }); },
			function() { $('.onhover', this).animate({ top: '80px', opacity: 0 }); }
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

		var acc = new EzBob.CgAccountModel(accountModel);
		var xhr = acc.save();

		if (!xhr) {
			EzBob.App.trigger('error', 'HMRC Account Saving Error');
			return false;
		} // if

		var self = this;

		BlockUi('on');

		xhr.always(function() { return BlockUi('off'); });

		xhr.fail(function(jqXhr, textStatus, errorThrown) {
			EzBob.ServerLog.warn('Failed to link a CG account with status', textStatus, ', jqXHR:', jqXhr, 'error thrown:', errorThrown);
			return EzBob.App.trigger('error', 'Failed to Save HMRC Account');
		});

		xhr.done(function(res) {
			if (res.error) {
				EzBob.App.trigger('error', res.error);
				return false;
			} // if

			try {
				self.model.add(acc);
			}
			catch (e) {
				// Silently ignore.
			} // try

			EzBob.App.trigger('info', 'HMRC Account Added Successfully');

			self.$el.find('#hmrc_user_id').val("");
			self.$el.find('#hmrc_password').val("");
			self.$el.find('#linkAccountDiv').hide();

			self.$el.find('#initialDiv').show();

			self.activeForm = self.$el.find('#hmrcLinkAccountForm');
			self.validator = EzBob.validateHmrcLinkForm(self.activeForm);

			self.inputChanged();

			self.trigger('completed');
			self.trigger('back');

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
}); // EzBob.HmrcAccountInfoView
