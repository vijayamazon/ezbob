(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.PayPointAccountButtonView = (function(_super) {

    __extends(PayPointAccountButtonView, _super);

    function PayPointAccountButtonView() {
      return PayPointAccountButtonView.__super__.constructor.apply(this, arguments);
    }

    PayPointAccountButtonView.prototype.initialize = function() {
      return PayPointAccountButtonView.__super__.initialize.call(this, {
        name: 'PayPoint',
        logoText: '',
        shops: this.model
      });
    };

    PayPointAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return PayPointAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.PayPointAccountInfoView = (function(_super) {

    __extends(PayPointAccountInfoView, _super);

    function PayPointAccountInfoView() {
      return PayPointAccountInfoView.__super__.constructor.apply(this, arguments);
    }

    PayPointAccountInfoView.prototype.template = '#PayPointAccoutInfoTemplate';

    PayPointAccountInfoView.prototype.events = {
      'click a.connect-payPoint': 'connect',
      "click a.back": "back",
      'change input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    PayPointAccountInfoView.prototype.ui = {
      mid: '#payPoint_mid',
      vpnPassword: '#payPoint_vpnPassword',
      remotePassword: '#payPoint_remotePassword',
      connect: 'a.connect-payPoint',
      form: 'form'
    };

    PayPointAccountInfoView.prototype.inputChanged = function() {
      var enabled;
      enabled = EzBob.Validation.checkForm(this.validator);
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    PayPointAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;
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
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        return EzBob.App.trigger('error', 'PayPoint Account Saving Error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        try {
          _this.model.add(acc);
        } catch (_error) {}
        EzBob.App.trigger('info', "PayPoint Account Added Successfully");
        _this.ui.mid.val("");
        _this.ui.vpnPassword.val("");
        _this.ui.remotePassword.val("");
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    PayPointAccountInfoView.prototype.render = function() {
      PayPointAccountInfoView.__super__.render.call(this);
      this.validator = EzBob.validatePayPointShopForm(this.ui.form);
      return this;
    };

    PayPointAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    PayPointAccountInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return "Link PayPoint Account";
    };

    return PayPointAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.PayPointAccountModel = (function(_super) {

    __extends(PayPointAccountModel, _super);

    function PayPointAccountModel() {
      return PayPointAccountModel.__super__.constructor.apply(this, arguments);
    }

    PayPointAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/PayPointMarketPlaces/Accounts";

    return PayPointAccountModel;

  })(Backbone.Model);

  EzBob.PayPointAccounts = (function(_super) {

    __extends(PayPointAccounts, _super);

    function PayPointAccounts() {
      return PayPointAccounts.__super__.constructor.apply(this, arguments);
    }

    PayPointAccounts.prototype.model = EzBob.PayPointAccountModel;

    PayPointAccounts.prototype.url = "" + window.gRootPath + "Customer/PayPointMarketPlaces/Accounts";

    return PayPointAccounts;

  })(Backbone.Collection);

}).call(this);
