(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.CreditLineDialog = (function(_super) {

    __extends(CreditLineDialog, _super);

    function CreditLineDialog() {
      return CreditLineDialog.__super__.constructor.apply(this, arguments);
    }

    CreditLineDialog.prototype.template = '#credit-line-dialog-template';

    CreditLineDialog.prototype.initialize = function() {
      this.cloneModel = this.model.clone();
      this.modelBinder = new Backbone.ModelBinder();
      return this.bindTo(this.cloneModel, "change:StartingFromDate", this.onChangeStartingDate, this);
    };

    CreditLineDialog.prototype.events = {
      'click .btnOk': 'save',
      'change #loan-type ': 'onChangeLoanType',
      'click #isLoanTypeSelectionAllowed': 'onChangeLoanTypeSelectionAllowed',
      'change #isLoanTypeSelectionAllowed': 'onChangeLoanTypeSelectionAllowed'
    };

    CreditLineDialog.prototype.ui = {
      form: "form"
    };

    CreditLineDialog.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Credit Line",
        position: "center",
        draggable: false,
        width: "73%",
        height: Math.max(window.innerHeight * 0.9, 600),
        dialogClass: "creditline-popup"
      };
    };

    CreditLineDialog.prototype.onChangeLoanTypeSelectionAllowed = function() {
      var controlledElements, _ref;
      controlledElements = '#loan-type, #repaymentPeriod';
      if ((_ref = this.cloneModel.get('IsLoanTypeSelectionAllowed')) === 1 || _ref === '1') {
        this.$el.find(controlledElements).attr('disabled', 'disabled');
        if (this.cloneModel.get('LoanTypeId') !== 1) {
          return this.cloneModel.set('LoanTypeId', 1);
        }
      } else {
        return this.$el.find(controlledElements).removeAttr('disabled');
      }
    };

    CreditLineDialog.prototype.onChangeStartingDate = function() {
      var endDate, startingDate;
      startingDate = moment.utc(this.cloneModel.get("StartingFromDate"), "DD/MM/YYYY");
      if (startingDate !== null) {
        endDate = startingDate.add('hours', this.cloneModel.get('OfferValidForHours'));
        return this.cloneModel.set("OfferValidateUntil", endDate.format('DD/MM/YYYY'));
      }
    };

    CreditLineDialog.prototype.onChangeLoanType = function() {
      var currentLoanType, loanTypeId;
      loanTypeId = +this.$el.find("#loan-type option:selected").val();
      currentLoanType = _.find(this.cloneModel.get("LoanTypes"), function(l) {
        return l.Id === loanTypeId;
      });
      if (loanTypeId == null) {
        return;
      }
      this.cloneModel.set("RepaymentPerion", currentLoanType.RepaymentPeriod);
      return this;
    };

    CreditLineDialog.prototype.save = function() {
      var action, post, postData,
        _this = this;
      if (!this.ui.form.valid()) {
        return;
      }
      postData = this.getPostData();
      action = "" + window.gRootPath + "Underwriter/ApplicationInfo/ChangeCreditLine";
      post = $.post(action, postData);
      post.done(function() {
        return _this.model.fetch();
      });
      return this.close();
    };

    CreditLineDialog.prototype.getPostData = function() {
      var data, m;
      m = this.cloneModel.toJSON();
      data = {
        id: m.CashRequestId,
        loanType: m.LoanTypeId,
        discountPlan: m.DiscountPlanId,
        amount: m.amount,
        interestRate: m.InterestRate,
        repaymentPeriod: m.RepaymentPerion,
        offerStart: m.StartingFromDate,
        offerValidUntil: m.OfferValidateUntil,
        useSetupFee: m.UseSetupFee,
        allowSendingEmail: m.AllowSendingEmail,
        isLoanTypeSelectionAllowed: m.IsLoanTypeSelectionAllowed
      };
      return data;
    };

    CreditLineDialog.prototype.bindings = {
      InterestRate: {
        selector: "input[name='interestRate']",
        converter: EzBob.BindingConverters.percentsFormat
      },
      RepaymentPerion: {
        selector: "input[name='repaymentPeriod']",
        converter: EzBob.BindingConverters.notNull
      },
      StartingFromDate: {
        selector: "input[name='startingFromDate']"
      },
      OfferValidateUntil: {
        selector: "input[name='offerValidUntil']"
      },
      UseSetupFee: {
        selector: "input[name='enableSetupFee']"
      },
      AllowSendingEmail: {
        selector: "input[name='allowSendingEmail']"
      },
      IsLoanTypeSelectionAllowed: {
        selector: "select[name='isLoanTypeSelectionAllowed']"
      },
      DiscountPlanId: "select[name='discount-plan']",
      LoanTypeId: "select[name='loan-type']",
      amount: {
        selector: "#offeredCreditLine",
        converter: EzBob.BindingConverters.moneyFormat
      }
    };

    CreditLineDialog.prototype.onRender = function() {
      this.modelBinder.bind(this.cloneModel, this.el, this.bindings);
      this.$el.find("#startingFromDate, #offerValidUntil").mask("99/99/9999").datepicker({
        autoclose: true,
        format: 'dd/mm/yyyy'
      });
      this.$el.find("#offeredCreditLine").autoNumeric(EzBob.moneyFormat);
      if (this.$el.find("#offeredCreditLine").val() === "-") {
        this.$el.find("#offeredCreditLine").val("");
      }
      this.$el.find("#interestRate").autoNumeric(EzBob.percentFormat);
      this.$el.find("#repaymentPeriod").numericOnly();
      return this.setValidator();
    };

    CreditLineDialog.prototype.setValidator = function() {
      return this.ui.form.validate({
        rules: {
          offeredCreditLine: {
            required: true,
            autonumericMin: EzBob.Config.XMinLoan,
            autonumericMax: EzBob.Config.MaxLoan
          },
          repaymentPeriod: {
            required: true,
            autonumericMin: 1
          },
          interestRate: {
            required: true,
            autonumericMin: 1,
            autonumericMax: 100
          },
          startingFromDate: {
            required: true,
            dateCheck: true
          },
          offerValidUntil: {
            required: true,
            dateCheck: true
          }
        },
        messages: {
          interestRate: {
            autonumericMin: "Interest Rate is below limit.",
            autonumericMax: "Interest Rate is above limit."
          },
          repaymentPeriod: {
            autonumericMin: "Repayment Period is below limit."
          },
          startingFromDate: {
            dateCheck: "Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012"
          },
          offerValidUntil: {
            dateCheck: "Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012"
          }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
      });
    };

    return CreditLineDialog;

  })(Backbone.Marionette.ItemView);

}).call(this);
