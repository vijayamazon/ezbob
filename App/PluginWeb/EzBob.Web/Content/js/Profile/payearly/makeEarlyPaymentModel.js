(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.MakeEarlyPaymentModel = (function(_super) {

    __extends(MakeEarlyPaymentModel, _super);

    function MakeEarlyPaymentModel() {
      return MakeEarlyPaymentModel.__super__.constructor.apply(this, arguments);
    }

    MakeEarlyPaymentModel.prototype.defaults = {
      amount: 0,
      paymentType: "loan",
      loanPaymentType: "full",
      defaultCard: true,
      url: "#"
    };

    MakeEarlyPaymentModel.prototype.initialize = function() {
      this.get("customer").on("fetch", this.recalculate, this);
      this.on("change:amount change:paymentType change:loan change:loanPaymentType", this.changed, this);
      this.on("change:paymentType", this.paymentTypeChanged, this);
      this.on("change:loanPaymentType", this.loanPaymentTypeChanged, this);
      this.on("change:loan", this.loanChanged, this);
      return this.recalculate();
    };

    MakeEarlyPaymentModel.prototype.recalculate = function() {
      var liveLoans, total;
      liveLoans = this.get("customer").get("ActiveLoans");
      total = this.get("customer").get("TotalEarlyPayment");
      return this.set({
        total: total,
        liveLoans: liveLoans,
        loan: (liveLoans ? liveLoans[0] : null),
        amount: this.get("customer").get("TotalEarlyPayment")
      });
    };

    MakeEarlyPaymentModel.prototype.paymentTypeChanged = function() {
      var loan, type;
      type = this.get("paymentType");
      switch (type) {
        case "total":
          return this.set("amount", this.get("customer").get("TotalEarlyPayment"));
        case "loan":
          loan = this.get("loan");
          if (loan) {
            return this.set("loanPaymentType", "full");
          }
          break;
      }
    };

    MakeEarlyPaymentModel.prototype.loanChanged = function() {
      return this.set({
        loanPaymentType: "full",
        paymentType: "loan"
      });
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
        case "other":
          amount = loan.get("TotalEarlyPayment");
          break;
      }
      return this.set("amount", amount);
    };

    MakeEarlyPaymentModel.prototype.changed = function() {
      var loan, url;
      loan = this.get("loan");
      if (!loan) {
        return;
      }
      url = window.gRootPath + "Customer/Paypoint/Pay?amount=" + this.get("amount");
      url += "&type=" + this.get("paymentType");
      url += "&loanPaymentType=" + this.get("loanPaymentType");
      url += "&loanId=" + loan.id;
      return this.set({
        url: url
      });
    };

    MakeEarlyPaymentModel.prototype.validate = function(attrs) {
      var loan, maxAmount;
      loan = attrs.loan;
      if (!loan) {
        return;
      }
      if (attrs.paymentType === "total") {
        return;
      }
      if (attrs.loanPaymentType !== "other") {
        return;
      }
      maxAmount = loan.get("TotalEarlyPayment");
      if (typeof attrs.amount !== "undefined") {
        if (attrs.amount > maxAmount) {
          return "too big";
        }
        if (attrs.amount < 30 && attrs.amount < maxAmount) {
          if (maxAmount > 30) {
            return "too little";
          }
          attrs.amount = maxAmount;
          return "too little";
        }
      }
    };

    return MakeEarlyPaymentModel;

  })(Backbone.Model);

}).call(this);
