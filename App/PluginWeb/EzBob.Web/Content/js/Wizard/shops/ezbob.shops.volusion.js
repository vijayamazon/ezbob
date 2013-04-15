(function() {
  var root, _ref, _ref1, _ref2, _ref3,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.VolusionAccountButtonView = (function(_super) {
    __extends(VolusionAccountButtonView, _super);

    function VolusionAccountButtonView() {
      _ref = VolusionAccountButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    VolusionAccountButtonView.prototype.initialize = function() {
      this.listView = new EzBob.StoreListView({
        model: this.model
      });
      return VolusionAccountButtonView.__super__.initialize.call(this, {
        name: 'Volusion',
        logoText: '',
        shops: this.model
      });
    };

    VolusionAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return VolusionAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.VolusionAccountInfoView = (function(_super) {
    __extends(VolusionAccountInfoView, _super);

    function VolusionAccountInfoView() {
      _ref1 = VolusionAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    VolusionAccountInfoView.prototype.template = '#VolusionAccoutInfoTemplate';

    VolusionAccountInfoView.prototype.events = {
      'click a.connect-volusion': 'connect',
      "click a.back": "back",
      'change input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    VolusionAccountInfoView.prototype.ui = {
      login: '#volusion_login',
      password: '#volusion_password',
      shopname: '#volusion_shopname',
      url: '#volusion_url',
      connect: 'a.connect-volusion',
      form: 'form'
    };

    VolusionAccountInfoView.prototype.inputChanged = function() {
      var enabled;

      enabled = this.ui.login.val() && this.ui.password.val();
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    VolusionAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;

      if (!this.validator.form()) {
        return false;
      }
      if (this.$el.find('a.connect-volusion').hasClass('disabled')) {
        return false;
      }
      acc = new EzBob.VolusionAccountModel({
        login: this.ui.login.val(),
        password: this.ui.password.val(),
        displayName: this.ui.shopname.val(),
        url: this.ui.url.val()
      });
      xhr = acc.save();
      if (!xhr) {
        EzBob.App.trigger('error', 'Volusion account saving error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        console.log(textStatus);
        return EzBob.App.trigger('error', 'Volusion account saving error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        _this.model.add(acc);
        EzBob.App.trigger('info', "Volusion Account added successfully");
        _this.ui.login.val("");
        _this.ui.password.val("");
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    VolusionAccountInfoView.prototype.render = function() {
      VolusionAccountInfoView.__super__.render.call(this);
      this.validator = EzBob.validateVolusionShopForm(this.ui.form);
      return this;
    };

    VolusionAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    return VolusionAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.VolusionAccountModel = (function(_super) {
    __extends(VolusionAccountModel, _super);

    function VolusionAccountModel() {
      _ref2 = VolusionAccountModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    VolusionAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/VolusionMarketPlaces/Accounts";

    return VolusionAccountModel;

  })(Backbone.Model);

  EzBob.VolusionAccounts = (function(_super) {
    __extends(VolusionAccounts, _super);

    function VolusionAccounts() {
      _ref3 = VolusionAccounts.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    VolusionAccounts.prototype.model = EzBob.VolusionAccountModel;

    VolusionAccounts.prototype.url = "" + window.gRootPath + "Customer/VolusionMarketPlaces/Accounts";

    return VolusionAccounts;

  })(Backbone.Collection);

}).call(this);
