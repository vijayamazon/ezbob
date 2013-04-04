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
      return this.modelBinder = new Backbone.ModelBinder();
    };

    CreditLineDialog.prototype.events = {
      'click .btnOk': 'save'
    };

    CreditLineDialog.prototype.ui = {
      form: "form"
    };

    CreditLineDialog.prototype.save = function() {
      var action, post, postData,
        _this = this;
      if (!this.ui.form.valid()) {
        return;
      }
      postData = this.serializeData();
      action = "" + window.gRootPath + "Underwriter/ApplicationInfo/ChangeCreditLine";
      post = $.post(action, postData);
      post.done(function() {
        return _this.model.fetch();
      });
      return this.close();
    };

    CreditLineDialog.prototype.serializeData = function() {
      var data, m;
      m = this.cloneModel.toJSON();
      data = {
        id: m.CashRequestId,
        loanType: m.LoanTypeId,
        amount: m.OfferedCreditLine,
        interestRate: m.InterestRate,
        repaymentPeriod: m.RepaymentPerion,
        offerStart: m.StartingFromDate,
        offerValidUntil: m.OfferValidateUntil,
        useSetupFee: m.UseSetupFee
      };
      return data;
    };

    CreditLineDialog.prototype.bindings = {
      OfferedCreditLine: {
        selector: "input[name='offeredCreditLine']"
      },
      InterestRate: {
        selector: "input[name='interestRate']",
        converter: EzBob.BidingConverters.percents
      },
      RepaymentPerion: {
        selector: "input[name='repaymentPeriod']"
      },
      StartingFromDate: {
        selector: "input[name='startingFromDate']"
      },
      OfferValidateUntil: {
        selector: "input[name='offerValidUntil']"
      },
      UseSetupFee: {
        selector: "input[name='enableSetupFee']"
      }
    };

    CreditLineDialog.prototype.onRender = function() {
      this.modelBinder.bind(this.cloneModel, this.el, this.bindings);
      this.$el.find("#startingFromDate, #offerValidUntil").datepicker({
        autoclose: true,
        format: 'dd/mm/yyyy'
      }).datepicker('show');
      this.$el.find("#offeredCreditLine").autoNumeric(EzBob.moneyFormat);
      this.$el.find("#interestRate").autoNumeric(EzBob.percentFormat);
      this.$el.find(".cashInput").cashEdit();
      return this.setValidator();
    };

    CreditLineDialog.prototype.setValidator = function() {
      return this.ui.form.validate({
        rules: {
          offeredCreditLine: {
            required: true,
            min: EzBob.Config.XMinLoan,
            max: EzBob.Config.MaxLoan
          },
          repaymentPeriod: {
            required: true,
            min: 1,
            max: 100
          },
          interestRate: {
            required: true,
            min: 1,
            max: 100
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
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
      });
    };

    return CreditLineDialog;

  })(Backbone.Marionette.ItemView);

}).call(this);
