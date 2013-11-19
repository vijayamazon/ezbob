(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.ApplyForLoanModel = (function(_super) {

    __extends(ApplyForLoanModel, _super);

    function ApplyForLoanModel() {
      return ApplyForLoanModel.__super__.constructor.apply(this, arguments);
    }

    ApplyForLoanModel.prototype.defaults = {
      neededCash: 100,
      maxCash: 15000,
      minCash: EzBob.Config.MinLoan,
      agree: false,
      agreement: false,
      CreditSum: 0,
      OfferValid: 0,
      OfferValidMintes: 0,
      loanType: 0,
      repaymentPeriod: 0,
      isLoanSourceEU: false
    };

    ApplyForLoanModel.prototype.validate = function(attrs) {
      var val;
      if (typeof attrs.neededCash !== "undefined") {
        val = attrs.neededCash;
        if (isNaN(val)) {
          attrs.neededCash = this.get("minCash");
        }
        if (val > this.get("maxCash")) {
          attrs.neededCash = this.get("maxCash");
        }
        if (val < this.get("minCash")) {
          attrs.neededCash = this.get("minCash");
        }
      }
      return false;
    };

    ApplyForLoanModel.prototype.initialize = function() {
      this.on("change:neededCash", this.buildUrl, this);
      return this.set({
        neededCash: this.get("maxCash"),
        minCash: (this.get("maxCash") > EzBob.Config.MinLoan ? EzBob.Config.MinLoan : EzBob.Config.XMinLoan),
        loanType: this.get("loanType"),
        repaymentPeriod: this.get("repaymentPeriod"),
        isLoanSourceEU: this.get("isLoanSourceEU")
      });
    };

    ApplyForLoanModel.prototype.buildUrl = function() {
      return this.set("url", "GetCash/GetTransactionId?loan_amount=" + (this.get('neededCash')) + "&loanType=" + (this.get('loanType')) + "&repaymentPeriod=" + (this.get('repaymentPeriod')));
    };

    return ApplyForLoanModel;

  })(Backbone.Model);

}).call(this);
