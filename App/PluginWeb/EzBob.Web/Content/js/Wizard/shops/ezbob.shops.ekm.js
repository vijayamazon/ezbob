(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.EKMAccountButtonView = (function(_super) {

    __extends(EKMAccountButtonView, _super);

    function EKMAccountButtonView() {
      return EKMAccountButtonView.__super__.constructor.apply(this, arguments);
    }

    EKMAccountButtonView.prototype.initialize = function() {
      this.listView = new EzBob.StoreListView({
        model: this.model
      });
      return EKMAccountButtonView.__super__.initialize.call(this, {
        name: 'ekm',
        logoText: ''
      });
    };

    EKMAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return EKMAccountButtonView;

  })(EzBob.StoreButtonWithListView);

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
      xhr.done(function(res) {
        BlockUi('off');
        if (res.error) {
          EzBob.App.trigger('error', 'ekm account saving error');
          return false;
        }
        _this.model.add(acc);
        EzBob.App.trigger('info', "EKM Account added successfully");
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    EKMAccountInfoView.prototype.render = function() {
      EKMAccountInfoView.__super__.render.call(this);
      this.validator = this.ui.form.validate({
        onfocusout: false,
        onfocusin: false,
        onclick: false,
        focusInvalid: false,
        ignoreTitle: true,
        rules: {
          ekm_login: {
            required: true,
            minlength: 2,
            maxlength: 30
          },
          ekm_password: {
            required: true,
            minlength: 2,
            maxlength: 30
          }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
      });
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
      return EKMAccountModel.__super__.constructor.apply(this, arguments);
    }

    EKMAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/EKMAccounts/Accounts";

    return EKMAccountModel;

  })(Backbone.Model);

  EzBob.EKMAccounts = (function(_super) {

    __extends(EKMAccounts, _super);

    function EKMAccounts() {
      return EKMAccounts.__super__.constructor.apply(this, arguments);
    }

    EKMAccounts.prototype.model = EzBob.EKMAccountModel;

    EKMAccounts.prototype.url = "" + window.gRootPath + "Customer/EKMAccounts/Accounts";

    return EKMAccounts;

  })(Backbone.Collection);

}).call(this);
