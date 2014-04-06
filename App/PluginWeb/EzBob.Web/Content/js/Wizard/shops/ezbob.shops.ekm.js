(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.EKMAccountInfoView = (function(_super) {

    __extends(EKMAccountInfoView, _super);

    function EKMAccountInfoView() {
      return EKMAccountInfoView.__super__.constructor.apply(this, arguments);
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
      enabled = EzBob.Validation.checkForm(this.validator);
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    EKMAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;
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
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        return EzBob.App.trigger('error', 'EKM Account Saving Error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        try {
          _this.model.add(acc);
        } catch (_error) {}
        EzBob.App.trigger('info', "EKM Account Added Successfully");
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
      EzBob.UiAction.registerView(this);
      return this;
    };

    EKMAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    EKMAccountInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return "Link EKM Account";
    };

    return EKMAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.EKMAccountModel = (function(_super) {

    __extends(EKMAccountModel, _super);

    function EKMAccountModel() {
      return EKMAccountModel.__super__.constructor.apply(this, arguments);
    }

    EKMAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/EkmMarketPlaces/Accounts";

    return EKMAccountModel;

  })(Backbone.Model);

  EzBob.EKMAccounts = (function(_super) {

    __extends(EKMAccounts, _super);

    function EKMAccounts() {
      return EKMAccounts.__super__.constructor.apply(this, arguments);
    }

    EKMAccounts.prototype.model = EzBob.EKMAccountModel;

    EKMAccounts.prototype.url = "" + window.gRootPath + "Customer/EkmMarketPlaces/Accounts";

    return EKMAccounts;

  })(Backbone.Collection);

}).call(this);
