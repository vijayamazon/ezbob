﻿var EzBob = EzBob || {};
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
	}, // defaults

	validate: function(attrs) {
		if (typeof attrs.neededCash === "undefined")
			return false;

		var val = attrs.neededCash;

		if (isNaN(val))
			attrs.neededCash = this.get("minCash");

		if (val > this.get("maxCash"))
			attrs.neededCash = this.get("maxCash");

		if (val < this.get("minCash"))
			attrs.neededCash = this.get("minCash");

		return false;
	}, // validate

	initialize: function() {
		this.on("change:neededCash", this.buildUrl, this);

		this.set({
			neededCash: this.get("maxCash"),
			minCash: (this.get("maxCash") > EzBob.Config.MinLoan ? EzBob.Config.MinLoan : EzBob.Config.XMinLoan),
			loanType: this.get("loanType"),
			repaymentPeriod: this.get("repaymentPeriod"),
			isLoanSourceEU: this.get("isLoanSourceEU"),
		});
	}, // initialize

	buildUrl: function() {
		return this.set("url", "GetCash/GetTransactionId?loan_amount=" + (this.get('neededCash')) + "&loanType=" + (this.get('loanType')) + "&repaymentPeriod=" + (this.get('repaymentPeriod')));
	}, // buildUrl
}); // EzBob.Profile.ApplyForLoanModel
