var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ApplyForLoanModel = Backbone.Model.extend({
	defaults: {
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
		isLoanSourceEU: false,
		approvedRepaymentPeriod: 0,
		isCustomerRepaymentPeriodSelectionAllowed: false,
		isLoanTypeSelectionAllowed: 0,
		isTest: false
	}, // defaults

	validate: function (attrs) {
		
		if (typeof attrs.neededCash === "undefined")
			return false;

		var val = attrs.neededCash;

		if (isNaN(val))
			this.set({ neededCash: this.get("minCash") }, { silent: true });

		if (val > this.get("maxCash"))
			this.set({ neededCash: this.get("maxCash") }, { silent: true });

		if (val < this.get("minCash"))
			this.set({ neededCash: this.get("minCash") }, { silent: true });

		return false;
	}, // validate

	initialize: function() {
		this.on("change:neededCash", this.buildUrl, this);
		var minCash = (this.get("maxCash") >= EzBob.Config.MinLoan ? EzBob.Config.MinLoan : EzBob.Config.XMinLoan);
		if (this.get('isTest')) {
			minCash = EzBob.Config.XMinLoan;
		}

		this.set({
			neededCash: this.get("maxCash"),
			minCash: minCash,
			loanType: this.get("loanType"),
			repaymentPeriod: this.get("repaymentPeriod"),
			isLoanSourceEU: this.get("isLoanSourceEU"),
			isLoanSourceCOSME: this.get('isLoanSourceCOSME')
		});
	}, // initialize

	buildUrl: function() {
		return this.set("url", "GetCash/GetTransactionId?loan_amount=" + (this.get('neededCash')) + "&loanType=" + (this.get('loanType')) + "&repaymentPeriod=" + (this.get('repaymentPeriod')));
	}, // buildUrl
}); // EzBob.Profile.ApplyForLoanModel
