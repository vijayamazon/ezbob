(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.FreeAgentAccountInfoView = (function(_super) {

    __extends(FreeAgentAccountInfoView, _super);

    function FreeAgentAccountInfoView() {
      return FreeAgentAccountInfoView.__super__.constructor.apply(this, arguments);
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
        $.colorbox.close();
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
      return FreeAgentAccountModel.__super__.constructor.apply(this, arguments);
    }

    FreeAgentAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/FreeAgentMarketPlaces/Accounts";

    return FreeAgentAccountModel;

  })(Backbone.Model);

  EzBob.FreeAgentAccounts = (function(_super) {

    __extends(FreeAgentAccounts, _super);

    function FreeAgentAccounts() {
      return FreeAgentAccounts.__super__.constructor.apply(this, arguments);
    }

    FreeAgentAccounts.prototype.model = EzBob.FreeAgentAccountModel;

    FreeAgentAccounts.prototype.url = "" + window.gRootPath + "Customer/FreeAgentMarketPlaces/Accounts";

    return FreeAgentAccounts;

  })(Backbone.Collection);

}).call(this);
