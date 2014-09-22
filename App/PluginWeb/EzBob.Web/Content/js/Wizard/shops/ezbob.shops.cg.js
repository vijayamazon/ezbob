var EzBob = EzBob || {};

EzBob.CgAccountInfoView = Backbone.Marionette.ItemView.extend({
	events: {
		'click a.back': 'back',
		'change input': 'inputChanged',
		'keyup input': 'inputChanged'
	},
	initialize: function (options) {
		this.uploadFilesDlg = null;
		this.accountType = options.accountType;
		this.template = '#' + this.accountType + 'AccountInfoTemplate';
		return this;
	},
	inputChanged: function () {
		var enabled;
		enabled = EzBob.Validation.checkForm(this.validator);
		return this.$el.find('a.connect-account').toggleClass('disabled', !enabled);
	},
	getVendorInfo: function () {
		if (!this.vendorInfo) {
			this.vendorInfo = EzBob.CgVendors.pure()[this.accountType];
		}
		return this.vendorInfo;
	},
	
	buildModel: function (bUploadMode) {
		var accountModel, elm, fi, func, oVendorInfo, propName, propVal, _i, _len, _ref;
		accountModel = $.parseJSON($('div#cg-account-model-template').text());
		oVendorInfo = this.getVendorInfo();
		accountModel.accountTypeName = this.accountType;
		_ref = oVendorInfo.SecurityData.Fields;
		for (_i = 0, _len = _ref.length; _i < _len; _i++) {
			fi = _ref[_i];
			if (fi.Default) {
				accountModel[fi.NodeName] = fi.Default;
			}
		}
		for (propName in accountModel) {
			propVal = accountModel[propName];
			elm = this.$el.find('#' + this.accountType.toLowerCase() + '_' + propName.toLowerCase());
			if (elm.length > 0) {
				accountModel[propName] = elm.val();
			}
		}
		if (oVendorInfo.ClientSide.LinkForm.OnBeforeLink.length) {
			func = new Function('accountModel', 'bUploadMode', oVendorInfo.ClientSide.LinkForm.OnBeforeLink.join("\n"));
			accountModel = func.call(null, accountModel, bUploadMode);
			if (!accountModel) {
				return null;
			}
		}
		delete accountModel.id;
		return accountModel;
	},
	
	connect: function () {
		var acc, accountModel, oVendorInfo, xhr;
		if (!EzBob.Validation.checkForm(this.validator)) {
			this.validator.form();
			return false;
		}
		if (this.$el.find('a.connect-account').hasClass('disabled')) {
			return false;
		}
		accountModel = this.buildModel(false);
		oVendorInfo = this.getVendorInfo();
		if (!accountModel) {
			EzBob.App.trigger('error', oVendorInfo.DisplayName + ' Account Data Validation Error');
			return false;
		}
		acc = new EzBob.CgAccountModel(accountModel);
		xhr = acc.save();
		if (!xhr) {
			EzBob.App.trigger('error', oVendorInfo.DisplayName + ' Account Saving Error');
			return false;
		}
		BlockUi('on');
		xhr.always((function (_this) {
			return function () {
				return BlockUi('off');
			};
		})(this));
		xhr.fail((function (_this) {
			return function (jqXHR, textStatus, errorThrown) {
				return EzBob.App.trigger('error', 'Failed to Save ' + oVendorInfo.DisplayName + ' Account');
			};
		})(this));
		xhr.done((function (_this) {
			return function (res) {
				var elm, propName, propVal;
				if (res.error) {
					EzBob.App.trigger('error', res.error);
					return false;
				}
				try {
					_this.model.add(acc);
				} catch (_error) { }
				EzBob.App.trigger('info', oVendorInfo.DisplayName + ' Account Added Successfully');
				for (propName in accountModel) {
					propVal = accountModel[propName];
					elm = _this.$el.find('#' + _this.accountType.toLowerCase() + '_' + propName.toLowerCase());
					if (elm.length > 0) {
						elm.val("");
					}
				}
				_this.inputChanged();
				_this.trigger('completed');
				return _this.trigger('back');
			};
		})(this));
		return false;
	},
	
	onRender: function () {
		var self;
		self = this;
		this.$el.find('a.connect-account').click(function (evt) {
			return self.connect();
		});
		this.validator = EzBob.validateCGShopForm(this.$el.find('form'), this.accountType);
		EzBob.UiAction.registerView(this);
		return this;
	},
	
	back: function () {
		this.trigger('back');
		return false;
	},
	getDocumentTitle: function () {
		EzBob.App.trigger('clear');
		return 'Link ' + this.getVendorInfo().DisplayName + ' Account';
	}
});

EzBob.CgAccountModel = Backbone.Model.extend({
	urlRoot: "" + window.gRootPath + "Customer/CGMarketPlaces/Accounts"
});

EzBob.CgAccounts = Backbone.Collection.extend({
	model: EzBob.CgAccountModel,
	accountType: '',
	url: function () {
		return ("" + window.gRootPath + "Customer/CGMarketPlaces/Accounts?atn=") + this.accountType;
	},
	initialize: function (data, options) {
		this.accountType = options.accountType;
		return this;
	}
});
