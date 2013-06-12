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
      return this.model.fetch().done(function() {
        return EzBob.App.trigger('ct:storebase.shop.connected');
      });
    };

    return FreeAgentAccountButtonView;

  })(EzBob.StoreButtonView);

  EzBob.FreeAgentAccountInfoView = (function(_super) {
    __extends(FreeAgentAccountInfoView, _super);

    function FreeAgentAccountInfoView() {
      _ref1 = FreeAgentAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    FreeAgentAccountInfoView.prototype.initialize = function(options) {
      var that;

      that = this;
      window.FreeAgentAccountAdded = function(result) {
        if (result.error) {
          EzBob.App.trigger('error', result.error);
        } else {
          EzBob.App.trigger('info', 'Congratulations. Free Agent account was added successfully.');
        }
        that.trigger('completed');
        return that.trigger('ready');
      };
      return false;
    };

    return FreeAgentAccountInfoView;

  })(Backbone.View);

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
