var EzBob = EzBob || {};

EzBob.PayPointAccountInfoView = Backbone.Marionette.ItemView.extend({
	template: '#PayPointAccoutInfoTemplate',
	events: {
		'click a.connect-payPoint': 'connect',
		"click a.back": "back",
		'change input': 'inputChanged',
		'keyup input': 'inputChanged'
	},
	ui: {
		mid: '#payPoint_mid',
		vpnPassword: '#payPoint_vpnPassword',
		remotePassword: '#payPoint_remotePassword',
		connect: 'a.connect-payPoint',
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
		if (this.$el.find('a.connect-payPoint').hasClass('disabled')) {
			return false;
		}
		acc = new EzBob.PayPointAccountModel({
			mid: this.ui.mid.val(),
			vpnPassword: this.ui.vpnPassword.val(),
			remotePassword: this.ui.remotePassword.val()
		});
		xhr = acc.save();
		if (!xhr) {
			EzBob.App.trigger('error', 'PayPoint Account Saving Error');
			return false;
		}
		BlockUi('on');
		xhr.always(function () {
			return BlockUi('off');
		});
		xhr.fail(function (jqXHR, textStatus, errorThrown) {
			return EzBob.App.trigger('error', 'PayPoint Account Saving Error');
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
				EzBob.App.trigger('info', "PayPoint Account Added Successfully");
				_this.ui.mid.val("");
				_this.ui.vpnPassword.val("");
				_this.ui.remotePassword.val("");
				_this.inputChanged();
				_this.trigger('completed');
				return _this.trigger('back');
			};
		})(this));
		return false;
	},
	onRender: function () {
		this.validator = EzBob.validatePayPointShopForm(this.ui.form);
		return this;
	},
	back: function () {
		this.trigger('back');
		return false;
	},
	getDocumentTitle: function () {
		EzBob.App.trigger('clear');
		return "Link PayPoint Account";
	}
});

EzBob.PayPointAccountModel = Backbone.Model.extend({
	urlRoot: "" + window.gRootPath + "Customer/PayPointMarketPlaces/Accounts"
});

EzBob.PayPointAccounts = Backbone.Collection.extend({
	model: EzBob.PayPointAccountModel,
	url: "" + window.gRootPath + "Customer/PayPointMarketPlaces/Accounts"
});
