(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.MakeEarlyPaymentModel = (function(_super) {
    __extends(MakeEarlyPaymentModel, _super);

    function MakeEarlyPaymentModel() {
      _ref = MakeEarlyPaymentModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    MakeEarlyPaymentModel.prototype.defaults = {
      amount: 0,
      paymentType: "loan",
      loanPaymentType: "full",
      rolloverPaymentType: "minimum",
      defaultCard: true,
      url: "#",
      isPayTotal: true,
      isPayRollover: false,
      isPayLoan: false,
      isPayTotalLate: false,
      isNextInterest: false
    };

    MakeEarlyPaymentModel.prototype.initialize = function() {
      this.get("customer").on("fetch", this.recalculate, this);
      this.on("change:amount change:paymentType change:loan change:loanPaymentType", this.changed, this);
      this.on("change:paymentType", this.paymentTypeChanged, this);
      this.on("change:loanPaymentType", this.loanPaymentTypeChanged, this);
      this.on("change:rolloverPaymentType", this.rolloverPaymentTypeChanged, this);
      this.on("change:loan", this.loanChanged, this);
      return this.recalculate();
    };

    MakeEarlyPaymentModel.prototype.recalculate = function() {
      var currentRollover, customer, liveLoans, liveRollovers, loan, total;

      customer = this.get("customer");
      liveRollovers = customer.get("ActiveRollovers");
      liveLoans = customer.get("ActiveLoans");
      total = customer.get("TotalEarlyPayment");
      loan = (liveLoans ? liveLoans[0] : null);
      currentRollover = this.calcCurrentRollover(loan);
      return this.set({
        total: total,
        liveLoans: liveLoans,
        rollovers: liveRollovers,
        loan: loan,
        amount: customer.get("TotalEarlyPayment"),
        currentRollover: currentRollover,
        hasLateLoans: customer.get("hasLateLoans"),
        totalLatePayment: customer.get("TotalLatePayment"),
        paymentType: liveRollovers.length > 0 ? "rollover" : "loan",
        isEarly: customer.get("IsEarly")
      });
    };

    MakeEarlyPaymentModel.prototype.calcCurrentRollover = function(loan) {
      var currentRollover, rollovers;

      rollovers = this.get("customer").get("ActiveRollovers").toJSON();
      currentRollover = _.where(rollovers, {
        LoanId: loan && loan.get('Id')
      })[0] || null;
      return currentRollover;
    };

    MakeEarlyPaymentModel.prototype.paymentTypeChanged = function(e) {
      var loan, type;

      type = this.get("paymentType");
      switch (type) {
        case "total":
          return this.set("amount", this.get("customer").get("TotalEarlyPayment"));
        case "totalLate":
          return this.set("amount", this.get("customer").get("TotalLatePayment"));
        case "loan":
          loan = this.get("loan");
          if (loan && this.get("liveLoans").length > 1) {
            return this.set("loanPaymentType", "full");
          }
          break;
        case "rollover":
          return this.set("rolloverPaymentType", "minimum");
      }
    };

    MakeEarlyPaymentModel.prototype.loanChanged = function() {
      var currentRollover, status;

      currentRollover = (this.calcCurrentRollover(this.get("loan"))) || null;
      status = this.get("loan") && this.get("loan").get("Status");
      this.set({
        currentRollover: currentRollover,
        loanPaymentType: status === "Late" ? "late" : "full"
      });
      if (currentRollover) {
        return this.set({
          amount: currentRollover && currentRollover.RolloverPayValue
        });
      }
    };

    MakeEarlyPaymentModel.prototype.rolloverPaymentTypeChanged = function() {
      return this.set("amount", this.get("currentRollover") && this.get("currentRollover").RolloverPayValue);
    };

    MakeEarlyPaymentModel.prototype.loanPaymentTypeChanged = function() {
      var amount, loan, type;

      type = this.get("loanPaymentType");
      loan = this.get("loan");
      amount = 0;
      if (!loan) {
        return;
      }
      switch (type) {
        case "full":
          amount = loan.get("TotalEarlyPayment");
          break;
        case "next":
          amount = loan.get("NextEarlyPayment");
          break;
        case "late":
          amount = loan.get("AmountDue");
          break;
        case "nextInterest":
          amount = loan.get("NextInterestPayment");
          break;
        case "other":
          amount = loan.get("TotalEarlyPayment");
          break;
      }
      return this.set("amount", amount);
    };

    MakeEarlyPaymentModel.prototype.changed = function() {
      var loan, paymentType, url;

      loan = this.get("loan");
      if (!loan) {
        return;
      }
      url = window.gRootPath + "Customer/Paypoint/Pay?amount=" + this.get("amount");
      url += "&type=" + this.get("paymentType");
      url += "&paymentType=" + this.getPaymentType();
      url += "&loanId=" + loan.id;
      url += "&rolloverId=" + (this.get("currentRollover") === !null ? this.get("currentRollover").Id : -1);
      this.set({
        url: url
      });
      paymentType = this.get('paymentType');
      return this.set({
        'isPayTotal': paymentType === 'total',
        'isPayRollover': paymentType === 'rollover',
        'isPayLoan': paymentType === 'loan',
        'isPayTotalLate': paymentType === 'totalLate',
        'isNextInterest': paymentType === 'nextInterest'
      });
    };

    MakeEarlyPaymentModel.prototype.getPaymentType = function() {
      if (this.get("paymentType") !== "rollover") {
        return this.get("loanPaymentType");
      } else {
        return this.get("rolloverPaymentType");
      }
    };

    return MakeEarlyPaymentModel;

  })(Backbone.Model);

}).call(this);
