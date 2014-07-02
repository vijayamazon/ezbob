(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.FundingView = (function(_super) {

    __extends(FundingView, _super);

    function FundingView() {
      return FundingView.__super__.constructor.apply(this, arguments);
    }

    FundingView.prototype.initialize = function() {
      var xhr,
        _this = this;
      this.model = new EzBob.Underwriter.FundingModel();
      this.model.on("change reset", this.render, this);
      this.requiredFunds = -1;
      this.model.fetch();
      xhr = $.post("" + window.gRootPath + "Underwriter/Funding/GetAvailableFundsInterval");
      return xhr.done(function(res) {
        var xhr1;
        xhr1 = $.post("" + window.gRootPath + "Underwriter/Funding/GetRequiredFunds");
        return xhr1.done(function(res) {
          _this.requiredFunds = res;
          _this.render();
          return _this.modelUpdater = setInterval(function() {
            return _this.model.fetch();
          }, res);
        });
      });
    };

    FundingView.prototype.template = "#funding-template";

    FundingView.prototype.events = {
      "click #addFundsBtn": "addFunds",
      "click #cancelManuallyAddedFundsBtn": "cancelManuallyAddedFunds"
    };

    FundingView.prototype.addFunds = function(e) {
      var d, that;
      that = this;
      d = new EzBob.Dialogs.PacentManual({
        model: this.model,
        title: "Pacnet Balance - Add Manual Funds",
        width: 400,
        postValueName: "amount",
        url: "Underwriter/Funding/SavePacnetManual",
        data: {
          limit: EzBob.Config.PacnetBalanceMaxManualChange
        },
        min: EzBob.Config.PacnetBalanceMaxManualChange * -1,
        max: EzBob.Config.PacnetBalanceMaxManualChange,
        required: true
      });
      d.render();
      return d.on("done", function() {
        return that.model.fetch();
      });
    };

    FundingView.prototype.cancelManuallyAddedFunds = function(e) {
      var d, that;
      that = this;
      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "UseSetupFee",
        title: "Pacnet Balance - Clear Manual Funds",
        width: 400,
        checkboxName: "I am sure",
        postValueName: "isSure",
        url: "Underwriter/Funding/DisableCurrentManualPacnetDeposits",
        data: {
          isSure: this.model.get("IsSure")
        }
      });
      d.render();
      return d.on("done", function() {
        return that.model.fetch();
      });
    };

    FundingView.prototype.onRender = function() {
      var availableFundsNum, li, tdHeader, tdValue;
      if (!$("body").hasClass("role-manager")) {
        this.$el.find('#addFundsBtn').hide();
        this.$el.find('#cancelManuallyAddedFundsBtn').hide();
      }
      li = $("[id='liFunding']");
      tdHeader = $("[id='available-funds-td-header']");
      tdValue = $("[id='available-funds-td-value']");
      availableFundsNum = this.model.get('AvailableFunds');
      if (this.requiredFunds > availableFundsNum) {
        li.addClass('available-funds-alert');
        tdHeader.addClass('available-funds-alert-text-color');
        return tdValue.addClass('available-funds-alert-text-color');
      } else {
        li.removeClass('available-funds-alert');
        tdHeader.removeClass('available-funds-alert-text-color');
        return tdValue.removeClass('available-funds-alert-text-color');
      }
    };

    FundingView.prototype.hide = function() {
      return this.$el.hide();
    };

    FundingView.prototype.show = function() {
      return this.$el.show();
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
      return FundingModel.__super__.constructor.apply(this, arguments);
    }

    FundingModel.prototype.urlRoot = function() {
      return "" + gRootPath + "Underwriter/Funding/GetCurrentFundingStatus";
    };

    return FundingModel;

  })(Backbone.Model);

}).call(this);
