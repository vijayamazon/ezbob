(function() {
  var root, _ref, _ref1, _ref2, _ref3,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.PlayAccountButtonView = (function(_super) {
    __extends(PlayAccountButtonView, _super);

    function PlayAccountButtonView() {
      _ref = PlayAccountButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    PlayAccountButtonView.prototype.initialize = function() {
      return PlayAccountButtonView.__super__.initialize.call(this, {
        name: 'Play',
        logoText: '',
        shops: this.model
      });
    };

    PlayAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return PlayAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.PlayAccountInfoView = (function(_super) {
    __extends(PlayAccountInfoView, _super);

    function PlayAccountInfoView() {
      this.inputChanged = __bind(this.inputChanged, this);      _ref1 = PlayAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    PlayAccountInfoView.prototype.template = '#PlayAccoutInfoTemplate';

    PlayAccountInfoView.prototype.events = {
      'click a.connect-play': 'connect',
      'click a.back': 'back',
      'change input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    PlayAccountInfoView.prototype.ui = {
      login: '#play_login',
      password: '#play_password',
      name: '#play_name',
      connect: 'a.connect-play',
      form: 'form'
    };

    PlayAccountInfoView.prototype.inputChanged = function() {
      var enabled;

      enabled = EzBob.Validation.checkForm(this.validator);
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    PlayAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;

      if (!this.validator.form()) {
        return false;
      }
      if (this.$el.find('a.connect-play').hasClass('disabled')) {
        return false;
      }
      acc = new EzBob.PlayAccountModel({
        login: this.ui.login.val(),
        password: this.ui.password.val(),
        name: this.ui.name.val()
      });
      xhr = acc.save();
      if (!xhr) {
        EzBob.App.trigger('error', 'Play.com Account Saving Error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        console.log(textStatus);
        return EzBob.App.trigger('error', 'Play.com Account Saving Error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        _this.model.add(acc);
        EzBob.App.trigger('info', "Play.com Account Added Successfully");
        _this.ui.login.val("");
        _this.ui.password.val("");
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    PlayAccountInfoView.prototype.render = function() {
      var oFieldStatusIcons;

      PlayAccountInfoView.__super__.render.call(this);
      oFieldStatusIcons = $('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
      });
      this.validator = EzBob.validatePlayShopForm(this.ui.form);
      return this;
    };

    PlayAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    PlayAccountInfoView.prototype.getDocumentTitle = function() {
      return "Link Play.com Account";
    };

    return PlayAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.PlayAccountModel = (function(_super) {
    __extends(PlayAccountModel, _super);

    function PlayAccountModel() {
      _ref2 = PlayAccountModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    PlayAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/PlayMarketPlaces/Accounts";

    return PlayAccountModel;

  })(Backbone.Model);

  EzBob.PlayAccounts = (function(_super) {
    __extends(PlayAccounts, _super);

    function PlayAccounts() {
      _ref3 = PlayAccounts.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    PlayAccounts.prototype.model = EzBob.PlayAccountModel;

    PlayAccounts.prototype.url = "" + window.gRootPath + "Customer/PlayMarketPlaces/Accounts";

    return PlayAccounts;

  })(Backbone.Collection);

}).call(this);
