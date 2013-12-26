(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.ManualPaymentView = (function(_super) {

    __extends(ManualPaymentView, _super);

    function ManualPaymentView() {
      return ManualPaymentView.__super__.constructor.apply(this, arguments);
    }

    ManualPaymentView.prototype.template = "#manualPayment-template";

    ManualPaymentView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Manual Payment",
        position: "center",
        draggable: false,
        width: "73%",
        height: Math.max(window.innerHeight * 0.9, 600),
        dialogClass: "manual-payment-popup"
      };
    };

    ManualPaymentView.prototype.onRender = function() {
      this.$el.find('.ezDateTime').splittedDateTime();
      this.validator = EzBob.validatemanualPaymentForm(this.ui.form);
      this.minAmount = 0.1;
      this.maxAmount = 0;
      this.updatePaymentData();
      return this;
    };

    ManualPaymentView.prototype.events = {
      "click .confirm": "confirmClicked",
      "click .uploadFiles": "uploadFilesClicked",
      "change [name='totalSumPaid']": "updatePaymentData",
      "change [name='paymentDate']": "updatePaymentData"
    };

    ManualPaymentView.prototype.ui = {
      form: '#payment-form',
      money: "[name='totalSumPaid']",
      date: "[name='paymentDate']",
      fees: "[name='fees']",
      interest: "[name='interest']",
      principal: "[name='principal']"
    };

    ManualPaymentView.prototype.confirmClicked = function() {
      if (!this.validator.form()) {
        return false;
      }
      this.trigger("addPayment", this.ui.form.serialize());
      return this.close();
    };

    ManualPaymentView.prototype.uploadFilesClicked = function() {
      $("#addNewDoc").click();
      return false;
    };

    ManualPaymentView.prototype.updatePaymentData = function() {
      var data, request,
        _this = this;
      data = {
        date: this.ui.date.val(),
        money: ValueOrDefault(this.ui.money.val(), 0),
        loanId: this.model.get("loanId")
      };
      request = $.get(window.gRootPath + "Underwriter/LoanHistory/GetPaymentInfo", data);
      return request.done(function(r) {
        var moneyTitle;
        if (r.error) {
          return;
        }
        _this.ui.fees.val(r.Fee);
        _this.ui.principal.val(r.Principal);
        _this.ui.interest.val(r.Interest);
        _this.ui.money.val(r.Amount);
        _this.minAmount = r.MinValue;
        _this.maxAmount = r.Balance;
        moneyTitle = "Minium value = " + _this.minAmount + ", maximum value = " + _this.maxAmount;
        _this.ui.money.attr('data-original-title', moneyTitle);
        _this.ui.money.tooltip({
          'trigger': 'hover',
          'title': moneyTitle
        });
        return _this.ui.money.tooltip("enable").tooltip('fixTitle');
      });
    };

    return ManualPaymentView;

  })(Backbone.Marionette.ItemView);

}).call(this);
