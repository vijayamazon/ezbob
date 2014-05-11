(function() {
  var root, _ref, _ref1, _ref2, _ref3,
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

    LoanOfferMultiplierModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=LoanOfferMultiplier";

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
        id = eventObject.target.id.substring(4);
        newValue = parseInt(eventObject.target.value);
      } else {
        id = eventObject.target.id.substring(6);
        newValue = parseInt(eventObject.target.value);
      }
      ranges = this.model.get('configTableEntries');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        if (row.Id.toString() === id) {
          if (typeIdentifier === "end") {
            row.End = newValue;
          }
          if (typeIdentifier === "sta") {
            row.Start = newValue;
          }
          if (typeIdentifier === "val") {
            row.Value = newValue;
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
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
        serializedModels: JSON.stringify(this.model.get('configTableEntries')),
        configTableType: 'LoanOfferMultiplier'
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
      ranges = this.model.get('configTableEntries');
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
      this.model.get('configTableEntries').push({
        Start: 0,
        Id: freeId,
        End: 0,
        Value: 0.0
      });
      this.render();
    };

    LoanOfferMultiplierView.prototype.serializeData = function() {
      var data;

      data = {
        configTableEntries: this.model.get('configTableEntries')
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
      var endObject, ranges, row, startObject, valueObject, _i, _len;

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
      ranges = this.model.get('configTableEntries');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        startObject = this.$el.find('#start_' + row.Id);
        if (startObject.length === 1) {
          startObject.numericOnly();
        }
        endObject = this.$el.find('#end_' + row.Id);
        if (endObject.length === 1) {
          endObject.numericOnly();
        }
        valueObject = this.$el.find('#value_' + row.Id);
        if (valueObject.length === 1) {
          valueObject.autoNumeric(EzBob.percentFormat).blur();
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

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

  EzBob.Underwriter.Settings.BasicInterestRateModel = (function(_super) {
    __extends(BasicInterestRateModel, _super);

    function BasicInterestRateModel() {
      _ref2 = BasicInterestRateModel.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    BasicInterestRateModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=BasicInterestRate";

    return BasicInterestRateModel;

  })(Backbone.Model);

  EzBob.Underwriter.Settings.BasicInterestRateView = (function(_super) {
    __extends(BasicInterestRateView, _super);

    function BasicInterestRateView() {
      _ref3 = BasicInterestRateView.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    BasicInterestRateView.prototype.template = "#basic-interest-rate-settings-template";

    BasicInterestRateView.prototype.initialize = function(options) {
      this.modelBinder = new Backbone.ModelBinder();
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    BasicInterestRateView.prototype.events = {
      "click .addRange": "addRange",
      "click .removeRange": "removeRange",
      "click #SaveBasicInterestRateSettings": "saveBasicInterestRateSettings",
      "click #CancelBasicInterestRateSettings": "update",
      "change .range-field": "valueChanged"
    };

    BasicInterestRateView.prototype.valueChanged = function(eventObject) {
      var id, newValue, ranges, row, typeIdentifier, _i, _len;

      typeIdentifier = eventObject.target.id.substring(0, 3);
      if (typeIdentifier === "end") {
        id = eventObject.target.id.substring(4);
        newValue = parseInt(eventObject.target.value);
      } else {
        id = eventObject.target.id.substring(6);
        newValue = parseInt(eventObject.target.value);
      }
      ranges = this.model.get('configTableEntries');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        if (row.Id.toString() === id) {
          if (typeIdentifier === "end") {
            row.End = newValue;
          }
          if (typeIdentifier === "sta") {
            row.Start = newValue;
          }
          if (typeIdentifier === "val") {
            row.Value = newValue;
          }
          return false;
        }
      }
      return false;
    };

    BasicInterestRateView.prototype.saveBasicInterestRateSettings = function() {
      var xhr,
        _this = this;

      BlockUi("on");
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/SaveConfigTable", {
        serializedModels: JSON.stringify(this.model.get('configTableEntries')),
        configTableType: 'BasicInterestRate'
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

    BasicInterestRateView.prototype.removeRange = function(eventObject) {
      var index, rangeId, ranges, row, _i, _len;

      rangeId = eventObject.target.getAttribute('basic-interest-rate-id');
      index = 0;
      ranges = this.model.get('configTableEntries');
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

    BasicInterestRateView.prototype.addRange = function(e, range) {
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
      this.model.get('configTableEntries').push({
        Start: 0,
        Id: freeId,
        End: 0,
        Value: 0.0
      });
      this.render();
    };

    BasicInterestRateView.prototype.serializeData = function() {
      var data;

      data = {
        configTableEntries: this.model.get('configTableEntries')
      };
      return data;
    };

    BasicInterestRateView.prototype.update = function() {
      var _this = this;

      return this.model.fetch().done(function() {
        return _this.render();
      });
    };

    BasicInterestRateView.prototype.onRender = function() {
      var endObject, ranges, row, startObject, valueObject, _i, _len;

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
      ranges = this.model.get('configTableEntries');
      for (_i = 0, _len = ranges.length; _i < _len; _i++) {
        row = ranges[_i];
        startObject = this.$el.find('#start_' + row.Id);
        if (startObject.length === 1) {
          startObject.numericOnly();
        }
        endObject = this.$el.find('#end_' + row.Id);
        if (endObject.length === 1) {
          endObject.numericOnly();
        }
        valueObject = this.$el.find('#value_' + row.Id);
        if (valueObject.length === 1) {
          valueObject.autoNumeric(EzBob.percentFormat).blur();
        }
      }
      return false;
    };

    BasicInterestRateView.prototype.show = function(type) {
      return this.$el.show();
    };

    BasicInterestRateView.prototype.hide = function() {
      return this.$el.hide();
    };

    return BasicInterestRateView;

  })(Backbone.Marionette.ItemView);

}).call(this);
