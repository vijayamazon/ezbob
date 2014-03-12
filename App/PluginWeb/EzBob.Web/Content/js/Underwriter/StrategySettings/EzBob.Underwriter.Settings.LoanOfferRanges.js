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

    LoanOfferRangesModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsBasicInterestRate";

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
      "click .removeRange": "removeRange",
      "click #SaveBasicInterestRateSettings": "saveBasicInterestRateSettings",
      "click #CancelBasicInterestRateSettings": "update"
    };

    LoanOfferRangesView.prototype.saveBasicInterestRateSettings = function() {
      var xhr,
        _this = this;

      BlockUi("on");
      debugger;
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveBasicInterestRate", {
        serializedModels: JSON.stringify(this.model.get('loanOfferRanges'))
      });
      xhr.done(function(res) {
        if (res.error) {
          return EzBob.App.trigger('error', res.error);
        }
      });
      xhr.always(function() {
        return BlockUi("off");
      });
      return false;
    };

    LoanOfferRangesView.prototype.removeRange = function(eventObject) {
      var rangeId;

      rangeId = eventObject.target.getAttribute('data-loan-offer-range-id');
      this.$el.find('#basicInterestRateRow_' + rangeId).remove();
    };

    LoanOfferRangesView.prototype.addRange = function(e, range) {
      debugger;
    };

    LoanOfferRangesView.prototype.serializeData = function() {
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
      var counter, found, x;

      if (!$("body").hasClass("role-manager")) {
        this.$el.find("select").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button").hide();
      }
      counter = 0;
      found = true;
      while (found) {
        x = this.$el.find('#startValue_' + counter);
        if (x.length === 1) {
          x.autoNumeric();
        } else {
          found = false;
        }
        counter++;
      }
      counter = 0;
      found = true;
      while (found) {
        x = this.$el.find('#endValue_' + counter);
        if (x.length === 1) {
          x.autoNumeric();
        } else {
          found = false;
        }
        counter++;
      }
      counter = 0;
      found = true;
      while (found) {
        x = this.$el.find('#interest_' + counter);
        if (x.length === 1) {
          x.autoNumeric();
        } else {
          found = false;
        }
        counter++;
      }
      return false;
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
