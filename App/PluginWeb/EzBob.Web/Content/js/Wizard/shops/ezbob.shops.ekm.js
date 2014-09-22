var EzBob = EzBob || {};

EzBob.EKMAccountInfoView = Backbone.Marionette.ItemView.extend({
	template: '#EKMAccoutInfoTemplate',
	events: {
		'click a.connect-ekm': 'connect',
		"click a.back": "back",
		'change input': 'inputChanged',
		'keyup input': 'inputChanged'
	},
	ui: {
		login: '#ekm_login',
		password: '#ekm_password',
		connect: 'a.connect-ekm',
		form: 'form'
	},
	inputChanged: function () {
		var enabled;
		enabled = EzBob.Validation.checkForm(this.validator);
		return this.ui.connect.toggleClass('disabled', !enabled);
	},
	connect: function () {
		var acc, xhr;
		if (!EzBob.Validation.checkForm(this.validator)) {
			this.validator.form();
			return false;
		}
		if (this.$el.find('a.connect-ekm').hasClass('disabled')) {
			return false;
		}
		acc = new EzBob.EKMAccountModel({
			login: this.ui.login.val(),
			password: this.ui.password.val()
		});
		xhr = acc.save();
		if (!xhr) {
			EzBob.App.trigger('error', 'EKM Account Saving Error');
			return false;
		}
		BlockUi('on');

		xhr.always(function () {
			return BlockUi('off');
		});
		
		xhr.fail(function (jqXHR, textStatus, errorThrown) {
			return EzBob.App.trigger('error', 'EKM Account Saving Error');
		});
		
		xhr.done((function (_this) {
			return function (res) {
				if (res.error) {
					EzBob.App.trigger('error', res.error);
					return false;
				}
				try {
					_this.model.add(acc);
				} catch (_error) { }
				EzBob.App.trigger('info', "EKM Account Added Successfully");
				_this.ui.login.val("");
				_this.ui.password.val("");
				_this.inputChanged();
				_this.trigger('completed');
				return _this.trigger('back');
			};
		})(this));
		return false;
	},
	onRender: function () {
		this.validator = EzBob.validateEkmShopForm(this.ui.form);
		EzBob.UiAction.registerView(this);
		return this;
	},
	back: function () {
		this.trigger('back');
		return false;
	},
	getDocumentTitle: function () {
		EzBob.App.trigger('clear');
		return "Link EKM Account";
	}
});

EzBob.EKMAccountModel = Backbone.Model.extend({
	urlRoot: "" + window.gRootPath + "Customer/EkmMarketPlaces/Accounts"
});

EzBob.EKMAccounts = Backbone.Collection.extend({
	model: EzBob.EKMAccountModel,
	url: "" + window.gRootPath + "Customer/EkmMarketPlaces/Accounts"
});