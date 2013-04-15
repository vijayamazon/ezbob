(function() {
  var root, _ref, _ref1, _ref2, _ref3,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.PayPointAccountButtonView = (function(_super) {
    __extends(PayPointAccountButtonView, _super);

    function PayPointAccountButtonView() {
      _ref = PayPointAccountButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    PayPointAccountButtonView.prototype.initialize = function() {
      this.listView = new EzBob.StoreListView({
        model: this.model
      });
      return PayPointAccountButtonView.__super__.initialize.call(this, {
        name: 'PayPoint',
        logoText: ''
      });
    };

    PayPointAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return PayPointAccountButtonView;

  })(EzBob.StoreButtonWithListView);

  EzBob.PayPointAccountInfoView = (function(_super) {
    __extends(PayPointAccountInfoView, _super);

    function PayPointAccountInfoView() {
      _ref1 = PayPointAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
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

      enabled = this.ui.mid.val() && this.ui.vpnPassword.val() && this.ui.remotePassword.val();
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    PayPointAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;

      if (!this.validator.form()) {
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
        EzBob.App.trigger('error', 'PayPoint account saving error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        console.log(textStatus);
        return EzBob.App.trigger('error', 'PayPoint account saving error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        _this.model.add(acc);
        EzBob.App.trigger('info', "PayPoint Account added successfully");
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

    return PayPointAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.PayPointAccountModel = (function(_super) {
    __extends(PayPointAccountModel, _super);

    function PayPointAccountModel() {
      _ref2 = PayPointAccountModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    PayPointAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/PayPointMarketPlaces/Accounts";

    return PayPointAccountModel;

  })(Backbone.Model);

  EzBob.PayPointAccounts = (function(_super) {
    __extends(PayPointAccounts, _super);

    function PayPointAccounts() {
      _ref3 = PayPointAccounts.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    PayPointAccounts.prototype.model = EzBob.PayPointAccountModel;

    PayPointAccounts.prototype.url = "" + window.gRootPath + "Customer/PayPointMarketPlaces/Accounts";

    return PayPointAccounts;

  })(Backbone.Collection);

}).call(this);
