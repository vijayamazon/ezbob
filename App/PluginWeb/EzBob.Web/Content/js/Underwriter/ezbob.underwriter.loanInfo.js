(function() {
  var ModelUpdater, root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.LoanInfoView = (function(_super) {

    __extends(LoanInfoView, _super);

    function LoanInfoView() {
      return LoanInfoView.__super__.constructor.apply(this, arguments);
    }

    LoanInfoView.prototype.template = "#profile-loan-info-template";

    LoanInfoView.prototype.initialize = function(options) {
      this.bindTo(this.model, "change reset", this.render, this);
      this.personalInfo = options.personalInfo;
      this.bindTo(this.personalInfo, "change", this.UpdateNewCreditLineState, this);
      this.bindTo(this.personalInfo, "change:CreditResult", this.changeCreditResult, this);
      return EzBob.App.vent.on('newCreditLine:done', this.showCreditLineDialog, this);
    };

    LoanInfoView.prototype.events = {
      "click [name='startingDateChangeButton']": "editStartingDate",
      "click [name='offerValidUntilDateChangeButton']": "editOfferValidUntilDate",
      "click [name='repaymentPeriodChangeButton']": "editRepaymentPeriod",
      "click [name='interestRateChangeButton']": "editInterestRate",
      "click [name='openCreditLineChangeButton']": "editOfferedCreditLine",
      "click [name='editDetails']": "editDetails",
      "click [name='setupFeeEditButton']": "editSetupFee",
      "click [name='newCreditLineBtn']": "runNewCreditLine",
      'click [name="allowSendingEmail"]': 'allowSendingEmail',
      'click [name="loanType"]': 'loanType'
    };

    LoanInfoView.prototype.editOfferValidUntilDate = function() {
      var d;
      d = new EzBob.Dialogs.DateEdit({
        model: this.model,
        propertyName: "OfferValidateUntil",
        title: "Offer valid until edit",
        postValueName: "date",
        url: "Underwriter/ApplicationInfo/ChangeOferValid",
        data: {
          id: this.model.get("CustomerId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.editStartingDate = function() {
      var d, that;
      that = this;
      d = new EzBob.Dialogs.DateEdit({
        model: this.model,
        propertyName: "StartingFromDate",
        title: "Starting date edit",
        postValueName: "date",
        url: "Underwriter/ApplicationInfo/ChangeStartingDate",
        data: {
          id: this.model.get("CustomerId")
        }
      });
      d.render();
      d.on("done", function() {
        return that.model.fetch();
      });
    };

    LoanInfoView.prototype.editRepaymentPeriod = function() {
      var d;
      d = new EzBob.Dialogs.IntegerEdit({
        model: this.model,
        propertyName: "RepaymentPerion",
        title: "Repayment period edit",
        postValueName: "period",
        url: "Underwriter/ApplicationInfo/ChangeCashRequestRepaymentPeriod",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.editOfferedCreditLine = function() {
      var d;
      d = new EzBob.Dialogs.OfferedCreditLineEdit({
        model: this.model,
        propertyName: "OfferedCreditLine",
        title: "Offer credit line edit",
        postValueName: "amount",
        url: "Underwriter/ApplicationInfo/ChangeCashRequestOpenCreditLine",
        data: {
          id: this.model.get("CashRequestId")
        },
        min: EzBob.Config.XMinLoan,
        max: EzBob.Config.MaxLoan
      });
      d.render();
    };

    LoanInfoView.prototype.editInterestRate = function() {
      var d;
      d = new EzBob.Dialogs.PercentsEdit({
        model: this.model,
        propertyName: "InterestRate",
        title: "Interest rate edit",
        postValueName: "interestRate",
        url: "Underwriter/ApplicationInfo/ChangeCashRequestInterestRate",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.editDetails = function() {
      var d;
      d = new EzBob.Dialogs.TextEdit({
        model: this.model,
        propertyName: "Details",
        title: "Details edit",
        postValueName: "details",
        url: "Underwriter/ApplicationInfo/SaveDetails",
        data: {
          id: this.model.get("CustomerId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.editSetupFee = function() {
      var d;
      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "UseSetupFee",
        title: "Setup Fee",
        postValueName: "enbaled",
        checkboxName: "Enable Setup Fee",
        url: "Underwriter/ApplicationInfo/ChangeSetupFee",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.runNewCreditLine = function(e) {
      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      this.RunCustomerCheck();
      return false;
    };

    LoanInfoView.prototype.RunCustomerCheck = function() {
      var _this = this;
      BlockUi("on");
      $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLine", {
        Id: this.model.get("CustomerId")
      }).done(function(response) {
        var updater;
        updater = new ModelUpdater(_this.personalInfo, 'CreditResult');
        return updater.start();
      }).fail(function(data) {
        return console.error(data.responseText);
      });
      return false;
    };

    LoanInfoView.prototype.allowSendingEmail = function() {
      var d;
      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "AllowSendingEmail",
        title: "Allow sending emails",
        postValueName: "enbaled",
        checkboxName: "Allow",
        url: "Underwriter/ApplicationInfo/AllowSendingEmails",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.loanType = function() {
      var d,
        _this = this;
      d = new EzBob.Dialogs.ComboEdit({
        model: this.model,
        propertyName: "LoanTypeId",
        title: "Loan type",
        comboValues: this.model.get('LoanTypes'),
        postValueName: "LoanType",
        url: "Underwriter/ApplicationInfo/LoanType",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
      d.on("done", function() {
        return _this.model.fetch();
      });
    };

    LoanInfoView.prototype.UpdateNewCreditLineState = function() {
      var collection, disabled, waiting;
      waiting = this.personalInfo.get("CreditResult") === "WaitingForDecision";
      collection = this.personalInfo.get("Disabled") !== 0;
      disabled = waiting || collection;
      return this.$el.find("input[name='newCreditLineBtn']").toggleClass("disabled", disabled);
    };

    LoanInfoView.prototype.serializeData = function() {
      return {
        m: this.model.toJSON()
      };
    };

    LoanInfoView.prototype.onRender = function() {
      return this.UpdateNewCreditLineState();
    };

    LoanInfoView.prototype.changeCreditResult = function() {
      this.model.fetch();
      return this.personalInfo.fetch();
    };

    LoanInfoView.prototype.showCreditLineDialog = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        var dialog;
        dialog = new EzBob.Underwriter.CreditLineDialog({
          model: _this.model
        });
        dialog.render();
        return EzBob.App.modal.show(dialog);
      });
    };

    return LoanInfoView;

  })(Backbone.Marionette.ItemView);

  ModelUpdater = (function() {

    function ModelUpdater(model, property) {
      this.model = model;
      this.property = property;
      this.start = __bind(this.start, this);

    }

    ModelUpdater.prototype.start = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.check();
      });
    };

    ModelUpdater.prototype.check = function() {
      if (!this.model.get(this.property).isNullOrEmpty()) {
        BlockUi('off');
        EzBob.App.vent.trigger('newCreditLine:done');
        return;
      } else {
        setTimeout(this.start, 1000);
      }
      return this;
    };

    return ModelUpdater;

  })();

  EzBob.Underwriter.LoanInfoModel = (function(_super) {

    __extends(LoanInfoModel, _super);

    function LoanInfoModel() {
      return LoanInfoModel.__super__.constructor.apply(this, arguments);
    }

    LoanInfoModel.prototype.idAttribute = "Id";

    LoanInfoModel.prototype.urlRoot = "" + window.gRootPath + "Underwriter/ApplicationInfo/Index";

    LoanInfoModel.prototype.initialize = function() {
      this.on("change:OfferValidateUntil", this.offerChanged, this);
      return this.on("change:LoanTypeId", this.loanTypeChanged, this);
    };

    LoanInfoModel.prototype.offerChanged = function() {
      var now, until_;
      until_ = moment(this.get("OfferValidateUntil"), "DD/MM/YYYY");
      now = moment();
      return this.set({
        OfferExpired: until_ < now
      });
    };

    LoanInfoModel.prototype.loanTypeChanged = function() {
      var id, type, types;
      types = this.get('LoanTypes');
      id = parseInt(this.get('LoanTypeId'), 10);
      type = _.find(types, function(t) {
        return t.value === id;
      });
      return this.set("LoanType", type.text);
    };

    return LoanInfoModel;

  })(Backbone.Model);

}).call(this);
