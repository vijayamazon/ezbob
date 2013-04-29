(function() {
  var root, _ref, _ref1, _ref2, _ref3,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.EKMAccountButtonView = (function(_super) {
    __extends(EKMAccountButtonView, _super);

    function EKMAccountButtonView() {
      _ref = EKMAccountButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    EKMAccountButtonView.prototype.initialize = function() {
      return EKMAccountButtonView.__super__.initialize.call(this, {
        name: 'EKM',
        logoText: '',
        shops: this.model
      });
    };

    EKMAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return EKMAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.EKMAccountInfoView = (function(_super) {
    __extends(EKMAccountInfoView, _super);

    function EKMAccountInfoView() {
      _ref1 = EKMAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    EKMAccountInfoView.prototype.template = '#EKMAccoutInfoTemplate';

    EKMAccountInfoView.prototype.events = {
      'click a.connect-ekm': 'connect',
      "click a.back": "back",
      'change input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    EKMAccountInfoView.prototype.ui = {
      login: '#ekm_login',
      password: '#ekm_password',
      connect: 'a.connect-ekm',
      form: 'form'
    };

    EKMAccountInfoView.prototype.inputChanged = function() {
      var enabled;

      enabled = this.ui.login.val() && this.ui.password.val();
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    EKMAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;

      if (!this.validator.form()) {
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
        EzBob.App.trigger('error', 'ekm account saving error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        console.log(textStatus);
        return EzBob.App.trigger('error', 'ekm account saving error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        _this.model.add(acc);
        EzBob.App.trigger('info', "EKM Account added successfully");
        _this.ui.login.val("");
        _this.ui.password.val("");
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    EKMAccountInfoView.prototype.render = function() {
      EKMAccountInfoView.__super__.render.call(this);
      this.validator = EzBob.validateEkmShopForm(this.ui.form);
      return this;
    };

    EKMAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    return EKMAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.EKMAccountModel = (function(_super) {
    __extends(EKMAccountModel, _super);

    function EKMAccountModel() {
      _ref2 = EKMAccountModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    EKMAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/EkmMarketPlaces/Accounts";

    return EKMAccountModel;

  })(Backbone.Model);

  EzBob.EKMAccounts = (function(_super) {
    __extends(EKMAccounts, _super);

    function EKMAccounts() {
      _ref3 = EKMAccounts.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    EKMAccounts.prototype.model = EzBob.EKMAccountModel;

    EKMAccounts.prototype.url = "" + window.gRootPath + "Customer/EkmMarketPlaces/Accounts";

    return EKMAccounts;

  })(Backbone.Collection);

}).call(this);
