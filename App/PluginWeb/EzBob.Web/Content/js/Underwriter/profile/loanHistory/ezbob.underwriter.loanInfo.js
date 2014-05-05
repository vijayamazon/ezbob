(function() {
  var ModelUpdater, root,
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.LoanInfoView = (function(_super) {

    __extends(LoanInfoView, _super);

    function LoanInfoView() {
      this.LoanTypeSelectionAllowedChanged = __bind(this.LoanTypeSelectionAllowedChanged, this);
      return LoanInfoView.__super__.constructor.apply(this, arguments);
    }

    LoanInfoView.prototype.template = "#profile-loan-info-template";

    LoanInfoView.prototype.initialize = function(options) {
      this.bindTo(this.model, "change reset sync", this.render, this);
      this.personalInfo = options.personalInfo;
      this.bindTo(this.personalInfo, "change", this.UpdateNewCreditLineState, this);
      this.bindTo(this.personalInfo, "change:CreditResult", this.changeCreditResult, this);
      EzBob.App.vent.on('newCreditLine:done', this.showCreditLineDialog, this);
      EzBob.App.vent.on('newCreditLine:error', this.showErrorDialog, this);
      EzBob.App.vent.on('newCreditLine:pass', this.showNothing, this);
      return this.parentView = options.parentView;
    };

    LoanInfoView.prototype.events = {
      "click [name='startingDateChangeButton']": "editStartingDate",
      "click [name='offerValidUntilDateChangeButton']": "editOfferValidUntilDate",
      "click [name='repaymentPeriodChangeButton']": "editRepaymentPeriod",
      "click [name='interestRateChangeButton']": "editInterestRate",
      "click [name='openCreditLineChangeButton']": "editOfferedCreditLine",
      "click [name='openPacnetManualButton']": "openPacnetManual",
      "click [name='clearPacnetManualButton']": "clearPacnetManual",
      "click [name='editDetails']": "editDetails",
      "click [name='setupFeeEditButton']": "editSetupFee",
      "click [name='brokerSetupFeeEditButton']": "editBrokerSetupFee",
      "click [name='manualSetupFeeEditAmountButton']": "editManualSetupFeeAmount",
      "click [name='manualSetupFeeEditPercentButton']": "editManualSetupFeePercent",
      "click [name='newCreditLineBtn']": "runNewCreditLine",
      'click [name="allowSendingEmail"]': 'allowSendingEmail',
      'click [name="loanType"]': 'loanType',
      'click [name="isLoanTypeSelectionAllowed"]': 'isLoanTypeSelectionAllowed',
      'click [name="discountPlan"]': 'discountPlan',
      'click [name="loanSource"]': 'loanSource',
      'click .create-loan-hidden-toggle': 'toggleCreateLoanHidden',
      'click #create-loan-hidden-btn': 'createLoanHidden'
    };

    LoanInfoView.prototype.toggleCreateLoanHidden = function(event) {
      if (!event.ctrlKey) {
        return;
      }
      return this.$el.find('#create-loan-hidden').toggleClass('hide');
    };

    LoanInfoView.prototype.createLoanHidden = function() {
      var nAmount, nCustomerID, oXhr, sDate,
        _this = this;
      nCustomerID = this.model.get('CustomerId');
      nAmount = parseInt(this.$el.find('#create-loan-hidden-amount').val(), 10) || 0;
      sDate = this.$el.find('#create-loan-hidden-date').val();
      if (nAmount <= 0) {
        EzBob.ShowMessageTimeout('Amount not specified.', 'Cannot create loan', 2);
        return;
      }
      if (!/^\d\d\d\d-\d\d-\d\d$/.test(sDate)) {
        EzBob.ShowMessageTimeout('Date not specified.', 'Cannot create loan', 2);
        return;
      }
      oXhr = $.post(window.gRootPath + 'Underwriter/ApplicationInfo/CreateLoanHidden', {
        nCustomerID: nCustomerID,
        nAmount: nAmount,
        sDate: sDate
      });
      oXhr.done(function(res) {
        if (res.success) {
          _this.$el.find('#create-loan-hidden-amount').val('');
          _this.$el.find('#create-loan-hidden-date').val('');
          _this.$el.find('#create-loan-hidden').addClass('hide');
          EzBob.ShowMessageTimeout('A loan has been created.', 'Loan created', 2);
        }
        if (res.error) {
          return EzBob.ShowMessage(res.error, 'Cannot create loan');
        } else {
          return EzBob.ShowMessage('Failed to create loan.', 'Cannot create loan');
        }
      });
      return oXhr.fail(function() {
        return EzBob.ShowMessage('Failed to create loan.', 'Cannot create loan');
      });
    };

    LoanInfoView.prototype.editOfferValidUntilDate = function() {
      var d;
      d = new EzBob.Dialogs.DateEdit({
        model: this.model,
        propertyName: "OfferValidateUntil",
        title: "Offer valid until edit",
        width: 370,
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
        width: 400,
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
        width: 400,
        postValueName: "period",
        url: "Underwriter/ApplicationInfo/ChangeCashRequestRepaymentPeriod",
        data: {
          id: this.model.get("CashRequestId")
        },
        required: true
      });
      d.render();
    };

    LoanInfoView.prototype.editOfferedCreditLine = function() {
      var that, view;
      that = this;
      view = new EzBob.Underwriter.CreditLineEditDialog({
        model: this.model
      });
      EzBob.App.jqmodal.show(view);
      view.on("showed", function() {
        return view.$el.find("input").focus();
      });
      view.on("done", function() {
        return that.model.fetch();
      });
    };

    LoanInfoView.prototype.openPacnetManual = function() {
      var d, that;
      that = this;
      d = new EzBob.Dialogs.PacentManual({
        model: this.model,
        title: "Pacnet Balance - Add Manual Funds",
        width: 400,
        postValueName: "amount",
        url: "Underwriter/ApplicationInfo/SavePacnetManual",
        data: {
          limit: EzBob.Config.PacnetBalanceMaxManualChange
        },
        min: EzBob.Config.PacnetBalanceMaxManualChange * -1,
        max: EzBob.Config.PacnetBalanceMaxManualChange,
        required: true
      });
      d.render();
      d.on("done", function() {
        return that.model.fetch();
      });
    };

    LoanInfoView.prototype.clearPacnetManual = function() {
      var d, that;
      that = this;
      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "UseSetupFee",
        title: "Pacnet Balance - Clear Manual Funds",
        width: 400,
        checkboxName: "I am sure",
        postValueName: "isSure",
        url: "Underwriter/ApplicationInfo/DisableTodaysPacnetManual",
        data: {
          isSure: this.model.get("IsSure")
        }
      });
      d.render();
      d.on("done", function() {
        return that.model.fetch();
      });
    };

    LoanInfoView.prototype.editInterestRate = function() {
      var d;
      d = new EzBob.Dialogs.PercentsEdit({
        model: this.model,
        propertyName: "InterestRate",
        title: "Interest rate edit",
        width: 400,
        postValueName: "interestRate",
        url: "Underwriter/ApplicationInfo/ChangeCashRequestInterestRate",
        data: {
          id: this.model.get("CashRequestId")
        },
        required: true
      });
      d.render();
    };

    LoanInfoView.prototype.editDetails = function() {
      var d;
      d = new EzBob.Dialogs.TextEdit({
        model: this.model,
        propertyName: "Details",
        title: "Details edit",
        width: 400,
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
        title: "Setup fee",
        width: 400,
        postValueName: "enbaled",
        checkboxName: "Enable Setup Fee",
        url: "Underwriter/ApplicationInfo/ChangeSetupFee",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.editBrokerSetupFee = function() {
      var d;
      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "UseBrokerSetupFee",
        title: "Broker Commission",
        width: 400,
        postValueName: "enbaled",
        checkboxName: "Enable broker commission",
        url: "Underwriter/ApplicationInfo/ChangeBrokerSetupFee",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.editManualSetupFeeAmount = function() {
      var d;
      d = new EzBob.Dialogs.PoundsNoDecimalsEdit({
        model: this.model,
        propertyName: "ManualSetupFeeAmount",
        title: "Manual setup fee amount edit",
        width: 400,
        postValueName: "manualAmount",
        url: "Underwriter/ApplicationInfo/ChangeManualSetupFeeAmount",
        data: {
          id: this.model.get("CashRequestId")
        },
        required: false
      });
      d.render();
    };

    LoanInfoView.prototype.editManualSetupFeePercent = function() {
      var d;
      d = new EzBob.Dialogs.PercentsEdit({
        model: this.model,
        propertyName: "ManualSetupFeePercent",
        title: "Manual setup fee percent edit",
        width: 400,
        postValueName: "manualPercent",
        url: "Underwriter/ApplicationInfo/ChangeManualSetupFeePercent",
        data: {
          id: this.model.get("CashRequestId")
        },
        required: false
      });
      d.render();
    };

    LoanInfoView.prototype.runNewCreditLine = function(e) {
      var el,
        _this = this;
      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      el = ($("<select/>")).css("height", "30px").css("width", "270px").append("<option value='1'>Skip everything, go to manual decision</option>").append("<option value='2'>Update everything except of MP's and go to manual decisions</option>").append("<option value='3'>Update everything and apply auto rules</option>").append("<option value='4'>Update everything and go to manual decision</option>");
      EzBob.ShowMessage(el, "New Credit Line Option", (function() {
        return _this.RunCustomerCheck(el.val());
      }), "OK", null, "Cancel");
      return false;
    };

    LoanInfoView.prototype.RunCustomerCheck = function(newCreditLineOption) {
      var that,
        _this = this;
      that = this;
      BlockUi("on");
      return $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLine", {
        Id: this.model.get("CustomerId"),
        NewCreditLineOption: newCreditLineOption
      }).done(function(response) {
        var updater;
        if (response.Message === "Go to new mode") {
          return $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLineNewMode1", {
            Id: that.model.get("CustomerId"),
            NewCreditLineOption: newCreditLineOption
          }).done(function(innerResponse) {
            return $.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLineNewMode2", {
              Id: that.model.get("CustomerId"),
              NewCreditLineOption: newCreditLineOption
            }).done(function(innerResponse2) {
              return that.personalInfo.fetch().done(function() {
                BlockUi('off');
                if (that.personalInfo.get('CreditResult') !== "WaitingForDecision") {
                  EzBob.App.vent.trigger('newCreditLine:pass');
                  return;
                }
                if (that.personalInfo.get('StrategyError') !== null && that.personalInfo.get('StrategyError') !== '') {
                  return EzBob.App.vent.trigger('newCreditLine:error', that.personalInfo.get('StrategyError'));
                } else {
                  return EzBob.App.vent.trigger('newCreditLine:done');
                }
              });
            });
          });
        } else {
          updater = new ModelUpdater(_this.personalInfo, 'IsMainStratFinished');
          return updater.start();
        }
      }).fail(function(data) {
        return console.error(data.responseText);
      });
    };

    LoanInfoView.prototype.allowSendingEmail = function() {
      var d;
      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "AllowSendingEmail",
        title: "Allow sending emails",
        width: 400,
        postValueName: "enbaled",
        checkboxName: "Allow",
        url: "Underwriter/ApplicationInfo/AllowSendingEmails",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
    };

    LoanInfoView.prototype.isLoanTypeSelectionAllowed = function() {
      var d,
        _this = this;
      d = new EzBob.Dialogs.ComboEdit({
        model: this.model,
        propertyName: "IsLoanTypeSelectionAllowed",
        title: "Customer selection",
        width: 400,
        postValueName: "loanTypeSelection",
        comboValues: [
          {
            value: 0,
            text: 'Disabled'
          }, {
            value: 1,
            text: 'Enabled'
          }
        ],
        url: "Underwriter/ApplicationInfo/IsLoanTypeSelectionAllowed",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
      d.on('done', function() {
        return _this.LoanTypeSelectionAllowedChanged();
      });
    };

    LoanInfoView.prototype.LoanTypeSelectionAllowedChanged = function() {
      var isCustomerRepaymentPeriodSelectionAllowed, _ref;
      isCustomerRepaymentPeriodSelectionAllowed = this.model.get('LoanSource').IsCustomerRepaymentPeriodSelectionAllowed;
      if (!isCustomerRepaymentPeriodSelectionAllowed || ((_ref = this.model.get('IsLoanTypeSelectionAllowed')) === 1 || _ref === '1')) {
        this.$el.find('button[name=loanType], button[name=repaymentPeriodChangeButton]').attr('disabled', 'disabled');
        if (this.model.get('LoanTypeId') !== 1) {
          return this.model.set('LoanTypeId', 1);
        }
      } else {
        return this.$el.find('button[name=loanType], button[name=repaymentPeriodChangeButton]').removeAttr('disabled');
      }
    };

    LoanInfoView.prototype.loanType = function() {
      var d,
        _this = this;
      d = new EzBob.Dialogs.ComboEdit({
        model: this.model,
        propertyName: "LoanTypeId",
        title: "Loan type",
        width: 400,
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

    LoanInfoView.prototype.loanSource = function() {
      var d,
        _this = this;
      d = new EzBob.Dialogs.ComboEdit({
        model: this.model,
        propertyName: "LoanSource.LoanSourceID",
        title: "Loan source",
        width: 400,
        comboValues: _.map(this.model.get('AllLoanSources'), function(ls) {
          return {
            value: ls.Id,
            text: ls.Name
          };
        }),
        postValueName: "LoanSourceID",
        url: "Underwriter/ApplicationInfo/LoanSource",
        data: {
          id: this.model.get("CashRequestId")
        }
      });
      d.render();
      d.on("done", function() {
        return _this.model.fetch();
      });
    };

    LoanInfoView.prototype.validateLoanSourceRelated = function() {
      var loanSourceModel, nAnnualTurnover, nCustomerReasonType, nEmployeeCount;
      loanSourceModel = this.model.get('LoanSource');
      this.validateInterestVsSource(loanSourceModel.MaxInterest);
      if (loanSourceModel.DefaultRepaymentPeriod === -1) {
        this.$el.find('button[name=repaymentPeriodChangeButton]').removeAttr('disabled');
      } else {
        this.$el.find('button[name=repaymentPeriodChangeButton]').attr('disabled', 'disabled');
      }
      this.parentView.clearDecisionNotes();
      if (loanSourceModel.MaxEmployeeCount !== -1) {
        nEmployeeCount = this.model.get('EmployeeCount');
        if (nEmployeeCount >= 0 && nEmployeeCount > loanSourceModel.MaxEmployeeCount) {
          this.parentView.appendDecisionNote('<div class=red>Employee count (' + nEmployeeCount + ') is greater than max employee count (' + loanSourceModel.MaxEmployeeCount + ') for this loan source.</div>');
        }
      }
      if (loanSourceModel.MaxAnnualTurnover !== -1) {
        nAnnualTurnover = this.model.get('AnnualTurnover');
        if (nAnnualTurnover >= 0 && nAnnualTurnover > loanSourceModel.MaxAnnualTurnover) {
          this.parentView.appendDecisionNote('<div class=red>Annual turnover (' + EzBob.formatPoundsNoDecimals(nAnnualTurnover) + ') is greater than max annual turnover (' + EzBob.formatPoundsNoDecimals(loanSourceModel.MaxAnnualTurnover) + ') for this loan source.</div>');
        }
      }
      if (loanSourceModel.AlertOnCustomerReasonType !== -1) {
        nCustomerReasonType = this.model.get('CustomerReasonType');
        if (loanSourceModel.AlertOnCustomerReasonType === nCustomerReasonType) {
          return this.parentView.appendDecisionNote('<div class=red>Please note customer reason: "' + this.model.get('CustomerReason') + '".</div>');
        }
      }
    };

    LoanInfoView.prototype.validateInterestVsSource = function(nMaxInterest) {
      var aryPercentList, nBaseRate, nChange, nPct, nRate, pct, sPercentList, _i, _len, _results;
      if (nMaxInterest === -1) {
        return;
      }
      this.$el.find('.interest-exceeds-max-by-loan-source').toggleClass('hide', this.model.get('InterestRate') <= nMaxInterest);
      this.$el.find('.discount-exceeds-max-by-loan-source').addClass('hide');
      sPercentList = this.model.get('DiscountPlanPercents');
      if (sPercentList === '') {
        return;
      }
      nBaseRate = this.model.get('InterestRate');
      aryPercentList = sPercentList.split(',');
      _results = [];
      for (_i = 0, _len = aryPercentList.length; _i < _len; _i++) {
        pct = aryPercentList[_i];
        if (pct[0] === '(') {
          pct = pct.substr(1);
        }
        nPct = parseFloat(pct);
        nChange = 100.0 + nPct;
        nRate = nBaseRate * nChange / 100.0;
        if (nRate > nMaxInterest) {
          this.$el.find('.discount-exceeds-max-by-loan-source').removeClass('hide');
          break;
        } else {
          _results.push(void 0);
        }
      }
      return _results;
    };

    LoanInfoView.prototype.discountPlan = function() {
      var d,
        _this = this;
      d = new EzBob.Dialogs.ComboEdit({
        model: this.model,
        propertyName: "DiscountPlanId",
        title: "Discount Plan",
        width: 400,
        comboValues: _.map(this.model.get('DiscountPlans'), function(v) {
          return {
            value: v.Id,
            text: v.Name
          };
        }),
        postValueName: "DiscountPlanId",
        url: "Underwriter/ApplicationInfo/DiscountPlan",
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
      var disabled, waiting;
      waiting = this.personalInfo.get("CreditResult") === "WaitingForDecision";
      disabled = waiting || !this.personalInfo.get("IsCustomerInEnabledStatus");
      $("input[name='newCreditLineBtn']").toggleClass("disabled", disabled);
      return $("#newCreditLineLnkId").toggleClass("disabled", disabled);
    };

    LoanInfoView.prototype.statuses = {};

    LoanInfoView.prototype.serializeData = function() {
      return {
        m: this.model.toJSON()
      };
    };

    LoanInfoView.prototype.onRender = function() {
      var _ref;
      this.$el.find(".tltp").tooltip();
      this.$el.find(".tltp-left").tooltip({
        placement: "left"
      });
      this.UpdateNewCreditLineState();
      this.LoanTypeSelectionAllowedChanged();
      if ((_ref = this.model.get('IsLoanTypeSelectionAllowed')) === 2 || _ref === '2') {
        this.$el.find('button[name=isLoanTypeSelectionAllowed]').attr('disabled', 'disabled');
      } else {
        this.$el.find('button[name=isLoanTypeSelectionAllowed]').removeAttr('disabled');
      }
      return this.validateLoanSourceRelated();
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
        return EzBob.App.jqmodal.show(dialog);
      });
    };

    LoanInfoView.prototype.showErrorDialog = function(errorMsg) {
      return EzBob.ShowMessage(errorMsg, "Something went wrong");
    };

    LoanInfoView.prototype.showNothing = function(errorMsg) {
      return this;
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
      if (Convert.toBool(this.model.get(this.property))) {
        BlockUi('off');
        if (this.model.get('CreditResult') !== "WaitingForDecision") {
          EzBob.App.vent.trigger('newCreditLine:pass');
          return;
        }
        if (this.model.get('StrategyError') !== null) {
          EzBob.App.vent.trigger('newCreditLine:error', this.model.get('StrategyError'));
        } else {
          EzBob.App.vent.trigger('newCreditLine:done');
        }
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
