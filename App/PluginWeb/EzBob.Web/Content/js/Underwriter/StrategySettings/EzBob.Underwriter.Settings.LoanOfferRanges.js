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
      "click #CancelBasicInterestRateSettings": "update",
      "change .range-field": "valueChanged"
    };

    LoanOfferRangesView.prototype.valueChanged = function(eventObject) {
      var id, newValue, ranges, row, typeIdentifier, _i, _len;

      typeIdentifier = eventObject.target.id.substring(0, 3);
      if (typeIdentifier === "end") {
        id = eventObject.target.id.substring(9);
        newValue = parseInt(eventObject.target.value);
      }
      if (typeIdentifier === "sta") {
        id = eventObject.target.id.substring(11);
        newValue = parseInt(eventObject.target.value);
      }
      if (typeIdentifier === "int") {
        id = eventObject.target.id.substring(9);
        newValue = parseFloat(eventObject.target.value);
      }
      ranges = this.model.get('loanOfferRanges');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        if (row.Id.toString() === id) {
          if (typeIdentifier === "end") {
            row.ToScore = newValue;
          }
          if (typeIdentifier === "sta") {
            row.FromScore = newValue;
          }
          if (typeIdentifier === "int") {
            row.LoanInterestBase = newValue;
          }
          return false;
        }
      }
      return false;
    };

    LoanOfferRangesView.prototype.saveBasicInterestRateSettings = function() {
      var xhr,
        _this = this;

      BlockUi("on");
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
      var index, rangeId, ranges, row, _i, _len;

      rangeId = eventObject.target.getAttribute('data-loan-offer-range-id');
      index = 0;
      ranges = this.model.get('loanOfferRanges');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        if (row.Id.toString() === rangeId) {
          ranges.splice(index, 1);
          this.render();
          return false;
        }
        index++;
      }
    };

    LoanOfferRangesView.prototype.addRange = function(e, range) {
      var freeId, t, verified;

      freeId = -1;
      verified = false;
      while (!verified) {
        t = this.$el.find('#basicInterestRateRow_' + freeId);
        if (t.length === 0) {
          verified = true;
        } else {
          freeId--;
        }
      }
      this.model.get('loanOfferRanges').push({
        FromScore: 0,
        Id: freeId,
        ToScore: 0,
        LoanInterestBase: 0.0
      });
      this.render();
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
      var fromScoreObject, loanInterestBaseObject, ranges, row, toScoreObject, _i, _len;

      if (!$("body").hasClass("role-manager")) {
        this.$el.find("select").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button").hide();
      }
      ranges = this.model.get('loanOfferRanges');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        fromScoreObject = this.$el.find('#startValue_' + row.Id);
        if (fromScoreObject.length === 1) {
          fromScoreObject.numericOnly();
        }
        toScoreObject = this.$el.find('#endValue_' + row.Id);
        if (toScoreObject.length === 1) {
          toScoreObject.numericOnly();
        }
        loanInterestBaseObject = this.$el.find('#interest_' + row.Id);
        if (loanInterestBaseObject.length === 1) {
          loanInterestBaseObject.autoNumeric(EzBob.percentFormat).blur();
        }
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
