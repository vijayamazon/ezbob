(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

  EzBob.Underwriter.Settings.LoanOfferMultiplierModel = (function(_super) {
    __extends(LoanOfferMultiplierModel, _super);

    function LoanOfferMultiplierModel() {
      _ref = LoanOfferMultiplierModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    LoanOfferMultiplierModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsLoanOfferMultiplier";

    return LoanOfferMultiplierModel;

  })(Backbone.Model);

  EzBob.Underwriter.Settings.LoanOfferMultiplierView = (function(_super) {
    __extends(LoanOfferMultiplierView, _super);

    function LoanOfferMultiplierView() {
      _ref1 = LoanOfferMultiplierView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    LoanOfferMultiplierView.prototype.template = "#loan-offer-multiplier-settings-template";

    LoanOfferMultiplierView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    LoanOfferMultiplierView.prototype.events = {
      "click .addRange": "addRange",
      "click .removeRange": "removeRange",
      "click #SaveLoanOfferMultiplierSettings": "saveLoanOfferMultiplierSettings",
      "click #CancelLoanOfferMultiplierSettings": "update",
      "change .range-field": "valueChanged"
    };

    LoanOfferMultiplierView.prototype.valueChanged = function(eventObject) {
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
      if (typeIdentifier === "mul") {
        id = eventObject.target.id.substring(11);
        newValue = parseFloat(eventObject.target.value);
      }
      ranges = this.model.get('loanOfferMultipliers');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        if (row.Id.toString() === id) {
          if (typeIdentifier === "end") {
            row.EndScore = newValue;
          }
          if (typeIdentifier === "sta") {
            row.StartScore = newValue;
          }
          if (typeIdentifier === "mul") {
            row.Multiplier = newValue;
          }
          return false;
        }
      }
      return false;
    };

    LoanOfferMultiplierView.prototype.saveLoanOfferMultiplierSettings = function() {
      var xhr,
        _this = this;

      BlockUi("on");
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveLoanOfferMultiplier", {
        serializedModels: JSON.stringify(this.model.get('loanOfferMultipliers'))
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

    LoanOfferMultiplierView.prototype.removeRange = function(eventObject) {
      var index, rangeId, ranges, row, _i, _len;

      rangeId = eventObject.target.getAttribute('loan-offer-multiplier-id');
      index = 0;
      ranges = this.model.get('loanOfferMultipliers');
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

    LoanOfferMultiplierView.prototype.addRange = function(e, range) {
      var freeId, t, verified;

      freeId = -1;
      verified = false;
      while (!verified) {
        t = this.$el.find('#loanOfferMultiplierRow_' + freeId);
        if (t.length === 0) {
          verified = true;
        } else {
          freeId--;
        }
      }
      this.model.get('loanOfferMultipliers').push({
        StartScore: 0,
        Id: freeId,
        EndScore: 0,
        Multiplier: 0.0
      });
      this.render();
    };

    LoanOfferMultiplierView.prototype.serializeData = function() {
      var data;

      data = {
        loanOfferMultipliers: this.model.get('loanOfferMultipliers')
      };
      return data;
    };

    LoanOfferMultiplierView.prototype.update = function() {
      var _this = this;

      return this.model.fetch().done(function() {
        return _this.render();
      });
    };

    LoanOfferMultiplierView.prototype.onRender = function() {
      var endScoreObject, multiplierObject, ranges, row, startScoreObject, _i, _len;

      if (!$("body").hasClass("role-manager")) {
        this.$el.find("select").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        this.$el.find("button").hide();
        this.$el.find("input").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
      }
      ranges = this.model.get('loanOfferMultipliers');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        startScoreObject = this.$el.find('#startScore_' + row.Id);
        if (startScoreObject.length === 1) {
          startScoreObject.numericOnly();
        }
        endScoreObject = this.$el.find('#endScore_' + row.Id);
        if (endScoreObject.length === 1) {
          endScoreObject.numericOnly();
        }
        multiplierObject = this.$el.find('#multiplier_' + row.Id);
        if (multiplierObject.length === 1) {
          multiplierObject.autoNumeric(EzBob.percentFormat).blur();
        }
      }
      return false;
    };

    LoanOfferMultiplierView.prototype.show = function(type) {
      return this.$el.show();
    };

    LoanOfferMultiplierView.prototype.hide = function() {
      return this.$el.hide();
    };

    return LoanOfferMultiplierView;

  })(Backbone.Marionette.ItemView);

}).call(this);
