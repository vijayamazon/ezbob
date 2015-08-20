var EzBob = EzBob || {};

EzBob.HmrcAccountInfoView = Backbone.Marionette.ItemView.extend({
	template: '#HMRCAccountInfoTemplate',
	initialize: function() {
		
	}, // initialize

	events: {
		'change input': 'inputChanged',
		'keyup input': 'inputChanged',
		'click a.hmrcBack': 'back',
		'click a.linkAccountBack': 'back',
		'click a.connect-account': 'connect',
		'click #linkHelpButton': 'getLinkHelp',
		'click #linkInfoButton': 'getLinkHelp',
	}, // events

	onRender: function() {
		return this;
	}, // onRender

	inputChanged: function() {
		this.activeForm = this.$el.find('#hmrcLinkAccountForm');
		this.validator = EzBob.validateHmrcLinkForm(this.activeForm);
		var enabled = EzBob.Validation.checkForm(this.validator);
		this.$el.find('a.connect-account').toggleClass('disabled', !enabled);
	}, // inputChanged

	back: function() {
		this.trigger('back');
		return false;
	}, // back

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

			self.$el.find('#hmrc_user_id').val('');
			self.$el.find('#hmrc_password').val('');
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

	getDocumentTitle: function () {
		EzBob.App.trigger('clear');
		return 'Link VAT Account';
	}, // getDocumentTitle

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
}); // EzBob.HmrcAccountInfoView
