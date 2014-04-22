(function() {
  var root, _ref, _ref1, _ref2,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.SageAccountInfoView = (function(_super) {
    __extends(SageAccountInfoView, _super);

    function SageAccountInfoView() {
      _ref = SageAccountInfoView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    SageAccountInfoView.prototype.initialize = function(options) {
      var that;

      that = this;
      window.SageAccountAdded = function(result) {
        if (result.error) {
          EzBob.App.trigger('error', result.error);
        } else {
          EzBob.App.trigger('info', 'Congratulations. Sage account was added successfully.');
        }
        $.colorbox.close();
        that.trigger('completed');
        return that.trigger('ready');
      };
      return false;
    };

    return SageAccountInfoView;

  })(Backbone.View);

  EzBob.SageAccountModel = (function(_super) {
    __extends(SageAccountModel, _super);

    function SageAccountModel() {
      _ref1 = SageAccountModel.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    SageAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/SageMarketPlaces/Accounts";

    return SageAccountModel;

  })(Backbone.Model);

  EzBob.SageAccounts = (function(_super) {
    __extends(SageAccounts, _super);

    function SageAccounts() {
      _ref2 = SageAccounts.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    SageAccounts.prototype.model = EzBob.SageAccountModel;

    SageAccounts.prototype.url = "" + window.gRootPath + "Customer/SageMarketPlaces/Accounts";

    return SageAccounts;

  })(Backbone.Collection);

}).call(this);
