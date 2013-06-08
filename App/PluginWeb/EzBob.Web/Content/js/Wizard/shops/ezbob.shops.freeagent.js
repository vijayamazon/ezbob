(function() {
  var root, _ref, _ref1, _ref2, _ref3,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.FreeAgentAccountButtonView = (function(_super) {
    __extends(FreeAgentAccountButtonView, _super);

    function FreeAgentAccountButtonView() {
      _ref = FreeAgentAccountButtonView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    FreeAgentAccountButtonView.prototype.initialize = function() {
      return FreeAgentAccountButtonView.__super__.initialize.call(this, {
        name: 'FreeAgent',
        logoText: '',
        shops: this.model
      });
    };

    FreeAgentAccountButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return FreeAgentAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.FreeAgentAccountInfoView = (function(_super) {
    __extends(FreeAgentAccountInfoView, _super);

    function FreeAgentAccountInfoView() {
      _ref1 = FreeAgentAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    FreeAgentAccountInfoView.prototype.template = '#FreeAgentAccoutInfoTemplate';

    FreeAgentAccountInfoView.prototype.events = {
      'click a.connect-freeagent': 'connect',
      "click a.back": "back",
      'change input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    FreeAgentAccountInfoView.prototype.ui = {
      displayName: '#freeagent_name',
      connect: 'a.connect-freeagent',
      form: 'form'
    };

    FreeAgentAccountInfoView.prototype.inputChanged = function() {
      var enabled;

      enabled = EzBob.Validation.checkForm(this.validator);
      return this.ui.connect.toggleClass('disabled', !enabled);
    };

    FreeAgentAccountInfoView.prototype.connect = function() {
      var acc, xhr,
        _this = this;

      if (!this.validator.form()) {
        return false;
      }
      if (this.$el.find('a.connect-freeagent').hasClass('disabled')) {
        return false;
      }
      acc = new EzBob.FreeAgentAccountModel({
        displayName: this.ui.displayName.val()
      });
      xhr = acc.save();
      if (!xhr) {
        EzBob.App.trigger('error', 'FreeAgent Account Saving Error');
        return false;
      }
      BlockUi('on');
      xhr.always(function() {
        return BlockUi('off');
      });
      xhr.fail(function(jqXHR, textStatus, errorThrown) {
        return EzBob.App.trigger('error', 'FreeAgent Account Saving Error');
      });
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return false;
        }
        try {
          _this.model.add(acc);
        } catch (_error) {}
        EzBob.App.trigger('info', "FreeAgent Account Added Successfully");
        _this.ui.displayName.val("");
        _this.inputChanged();
        _this.trigger('completed');
        return _this.trigger('back');
      });
      return false;
    };

    FreeAgentAccountInfoView.prototype.render = function() {
      var oFieldStatusIcons;

      FreeAgentAccountInfoView.__super__.render.call(this);
      oFieldStatusIcons = $('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
      });
      this.validator = EzBob.validateFreeAgentAccountForm(this.ui.form);
      return this;
    };

    FreeAgentAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    FreeAgentAccountInfoView.prototype.getDocumentTitle = function() {
      return "Link FreeAgent Account";
    };

    return FreeAgentAccountInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.FreeAgentAccountModel = (function(_super) {
    __extends(FreeAgentAccountModel, _super);

    function FreeAgentAccountModel() {
      _ref2 = FreeAgentAccountModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    FreeAgentAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/FreeAgentMarketPlaces/Accounts";

    return FreeAgentAccountModel;

  })(Backbone.Model);

  EzBob.FreeAgentAccounts = (function(_super) {
    __extends(FreeAgentAccounts, _super);

    function FreeAgentAccounts() {
      _ref3 = FreeAgentAccounts.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    FreeAgentAccounts.prototype.model = EzBob.FreeAgentAccountModel;

    FreeAgentAccounts.prototype.url = "" + window.gRootPath + "Customer/FreeAgentMarketPlaces/Accounts";

    return FreeAgentAccounts;

  })(Backbone.Collection);

}).call(this);
