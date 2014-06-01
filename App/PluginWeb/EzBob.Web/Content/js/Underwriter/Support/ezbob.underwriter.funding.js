(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.FundingView = (function(_super) {
    __extends(FundingView, _super);

    function FundingView() {
      _ref = FundingView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    FundingView.prototype.initialize = function() {
      this.model = new EzBob.Underwriter.FundingModel();
      this.model.on("change reset", this.render, this);
      return this.model.fetch();
    };

    FundingView.prototype.template = "#funding-template";

    FundingView.prototype.events = {
      "click #addFundsBtn": "addFunds",
      "click #cancelManuallyAddedFundsBtn": "cancelManuallyAddedFunds"
    };

    FundingView.prototype.onRender = function() {
      return console.log('placeholder');
    };

    FundingView.prototype.addFunds = function(e) {
      return console.log('placeholder');
    };

    FundingView.prototype.cancelManuallyAddedFunds = function(e) {
      return console.log('placeholder');
    };

    FundingView.prototype.hide = function() {
      this.$el.hide();
      clearInterval(this.modelUpdater);
      return BlockUi('off');
    };

    FundingView.prototype.show = function() {
      var _this = this;

      this.$el.show();
      return this.modelUpdater = setInterval(function() {
        return _this.model.fetch();
      }, 2000);
    };

    FundingView.prototype.serializeData = function() {
      return {
        model: this.model
      };
    };

    return FundingView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.FundingModel = (function(_super) {
    __extends(FundingModel, _super);

    function FundingModel() {
      _ref1 = FundingModel.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    FundingModel.prototype.urlRoot = function() {
      return "" + gRootPath + "Underwriter/Funding/GetCurrentFundingStatus";
    };

    return FundingModel;

  })(Backbone.Model);

}).call(this);
