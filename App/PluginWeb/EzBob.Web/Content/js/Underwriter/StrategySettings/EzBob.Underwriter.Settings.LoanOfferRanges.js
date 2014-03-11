(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

  EzBob.Underwriter.Settings.LoanOfferRangesModel = (function(_super) {
    __extends(LoanOfferRangesModel, _super);

    function LoanOfferRangesModel() {
      _ref = LoanOfferRangesModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    LoanOfferRangesModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsLoanOfferRanges";

    return LoanOfferRangesModel;

  })(Backbone.Model);

  EzBob.Underwriter.Settings.LoanOfferRangesView = (function(_super) {
    __extends(LoanOfferRangesView, _super);

    function LoanOfferRangesView() {
      _ref1 = LoanOfferRangesView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    LoanOfferRangesView.prototype.template = "#loan-offer-ranges-settings-template";

    LoanOfferRangesView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    LoanOfferRangesView.prototype.events = {
      "click .addRange": "addRange",
      "click .removeRange": "removeRange"
    };

    LoanOfferRangesView.prototype.removeRange = function(rangeId) {};

    LoanOfferRangesView.prototype.addRange = function(e, range) {};

    LoanOfferRangesView.prototype.serializeData = function() {
      debugger;
      var data;

      data = {
        loanOfferRanges: this.model.get('loanOfferRanges')
      };
      return data;
    };

    LoanOfferRangesView.prototype.update = function() {
      var _this = this;

      return this.model.fetch().done(function() {
        return _this.render();
      });
    };

    LoanOfferRangesView.prototype.onRender = function() {
      if (!$("body").hasClass("role-manager")) {
        this.$el.find("select").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        return this.$el.find("button").hide();
      }
    };

    LoanOfferRangesView.prototype.show = function(type) {
      return this.$el.show();
    };

    LoanOfferRangesView.prototype.hide = function() {
      return this.$el.hide();
    };

    return LoanOfferRangesView;

  })(Backbone.Marionette.ItemView);

}).call(this);
