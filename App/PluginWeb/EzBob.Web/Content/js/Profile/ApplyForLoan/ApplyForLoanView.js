var EzBob = EzBob || {};

EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ApplyForLoanView = Backbone.Marionette.ItemView.extend({
	template: "#apply-forloan-template",

	initialize: function(options) {
		this.customer = options.customer;

		if (this.customer.get("CreditSum") < EzBob.Config.XMinLoan) {
			this.trigger("back");
			document.location.href = "#";
			return;
		} // if

		this.fixed = this.customer.get('IsLoanDetailsFixed');

		this.isLoanTypeSelectionAllowed = this.customer.get('IsLoanTypeSelectionAllowed');

		this.currentLoanTypeID = 1; // for backward compatibility
		this.currentRepaymentPeriod = this.customer.get('LastApprovedRepaymentPeriod');

		this.recalculateThrottled = _.debounce(this.recalculateSchedule, 250);

		this.timerId = setInterval(_.bind(this.refreshTimer, this), 1000);

		this.model.set({
			"CreditSum": this.customer.get("CreditSum"),
			"OfferValid": this.customer.offerValidFormatted()
		});

		this.agreementView = null;

		if (this.fixed)
			return;

		this.model.on("change:neededCash", this.neededCashChanged, this);
		this.isLoanSourceEU = options.model.get("isLoanSourceEU");
	}, // initialize

	events: {
		"click .submit": "submit",
		"change .preAgreementTermsRead": "preAgreementTermsReadChange",
		"change .agreementTermsRead": "showSubmit",
		"change .euAgreementTermsRead": "showSubmit",
		"change .directorConsentRead": "showSubmit",
		"change #signedName": "showSubmit",
		"blur #signedName": "showSubmit",
		"keyup #signedName": "showSubmit",
		"paste #signedName": "showSubmit",
		"click .print": "print"
	}, // events

	ui: {
		submit: ".submit",
		agreement: ".agreement",
		form: "form"
	}, // ui

	preAgreementTermsReadChange: function() {
		var readPreAgreement = $(".preAgreementTermsRead").is(":checked");

		$(".agreementTermsRead").attr("disabled", !readPreAgreement);

		if (readPreAgreement)
			this.$el.find("a[href=\"#tab4\"]").tab("show");
		else {
			this.$el.find("a[href=\"#tab3\"]").tab("show");
			$(".agreementTermsRead").attr("checked", false);
		} // if

		return this.showSubmit();
	}, // preAgreementTermsReadChange

	loanSelectionChanged: function(e) {
		this.currentRepaymentPeriod = this.$('#loan-sliders .period-slider').slider('value');

		var amount = this.$('#loan-sliders .amount-slider').slider('value');

		this.model.set("neededCash", parseInt(amount, 10));
		this.model.set("loanType", this.currentLoanTypeID);
		this.model.set("repaymentPeriod", this.currentRepaymentPeriod);

		this.neededCashChanged(true);
	}, // loanSelectionChanged

	showSubmit: function() {
		var enabled = EzBob.Validation.checkForm(this.validator);
		this.model.set("agree", enabled);
		this.ui.submit.toggleClass("disabled", !enabled);
	}, // showSubmit

	recalculateSchedule: function(args) {
		var self = this;
		var val = args.value;

		/*
		unless args.reloadSelectedOnly is true
		$.getJSON("#{window.gRootPath}Customer/Schedule/CalculateAll?amount=#{parseInt(val)}").done (data) =>
		for loanKey, offer of data
		$('#loan-type-' + loanKey + ' .Interest').text EzBob.formatPounds offer.TotalInterest
		$('#loan-type-' + loanKey + ' .Total').text EzBob.formatPounds offer.Total
		*/

		BlockUi("on", this.$el.find('#block-loan-schedule'));
		BlockUi("on", this.$el.find('#block-agreement'));

		var sMoreParams = '&loanType=' + this.currentLoanTypeID + '&repaymentPeriod=' + this.currentRepaymentPeriod;

		$.getJSON(("" + window.gRootPath + "Customer/Schedule/Calculate?amount=" + (parseInt(val))) + sMoreParams).done(function(data) {
			self.renderSchedule(data);

			BlockUi("off", self.$el.find('#block-loan-schedule'));
			BlockUi("off", self.$el.find('#block-agreement'));
		});
	}, // recalculateSchedule

	renderSchedule: function(schedule) {
		this.lastPaymentDate = moment(schedule.Schedule[schedule.Schedule.length - 1].Date);

		var scheduleView = new EzBob.LoanScheduleView({
			el: this.$el.find(".loan-schedule"),
			schedule: schedule,
			isShowGift: false,
			isShowExportBlock: false,
			isShowExceedMaxInterestForSource: false,
		    isPersonal: this.customer.get("CustomerPersonalInfo").TypeOfBusiness in [0, 4, 2]
		});

		scheduleView.render();

		this.createAgreementView(schedule.Agreement);
	}, // renderSchedule

	neededCashChanged: function(reloadSelectedOnly) {
		this.$el.find('.preAgreementTermsRead, .agreementTermsRead, .euAgreementTermsRead').prop('checked', false);

		var value = this.model.get("neededCash");

		this.ui.submit.attr("href", this.model.get("url"));

		return this.recalculateThrottled({
			value: value,
			reloadSelectedOnly: reloadSelectedOnly
		});
	}, // neededCashChanged

	onRender: function() {
		if (this.fixed)
			this.$(".cash-question").hide();

		if (!this.isLoanTypeSelectionAllowed in [ 1, '1'] || this.isLoanSourceEU)
			this.$('.duration-select-allowed').hide();

		if (!this.isLoanSourceEU)
			this.$('.eu-agreement-section').hide();

		var self = this;

		if (this.model.get('isCurrentCashRequestFromQuickOffer'))
			this.$('.loan-amount-header-start').text('Confirm loan amount');
		else {
			this.$('.quick-offer-section').remove();

			InitAmountPeriodSliders({
				container: this.$('#loan-sliders'),
				amount: {
					min: this.model.get('minCash'),
					max: this.model.get('maxCash'),
					start: this.model.get('maxCash'),
					step: 100,
				},
				period: {
					min: 3,
					max: 12,
					start: this.model.get('repaymentPeriod'),
					step: 1,
					hide: !this.isLoanTypeSelectionAllowed in [ 1, '1' ] || this.isLoanSourceEU,
				},
				callback: function(ignored, sEvent) {
					if (sEvent === 'change')
						self.loanSelectionChanged();
				} // callback
			});
		} // if

		this.neededCashChanged();

		this.$el.find("img[rel]").setPopover('right');
		this.$el.find("li[rel]").setPopover('left');

		var pi = this.customer.get('CustomerPersonalInfo');

		this.$el.find('#signedName').attr('maxlength', pi.Fullname.length + 10);

		this.validator = EzBob.validateLoanLegalForm(this.ui.form, [pi.FirstName, pi.Surname]);

		this.$el.find('form.LoanLegal').submit(function(evt) {
			return self.submit(evt);
		});

		EzBob.UiAction.registerView(this);

		return this;
	}, // onRender

	refreshTimer: function() {
		return this.$el.find(".offerValidFor").text(this.customer.offerValidFormatted());
	}, // refreshTimer

	submit: function(e) {
		e.preventDefault();

		var creditSum = this.model.get("neededCash");
		var max = this.model.get("maxCash");
		var min = this.model.get("minCash");

		this.model.set("neededCash", creditSum);
		this.model.set("loanType", this.currentLoanTypeID);
		this.model.set("repaymentPeriod", this.currentRepaymentPeriod);

		if (creditSum > max || creditSum < min)
			return false;

		var enabled = EzBob.Validation.checkForm(this.validator);

		if (!enabled) {
			this.showSubmit();
			return false;
		} // if

		this.$el.find('#customerSelectedTerm').val(this.currentRepaymentPeriod);

		this.trigger("submit");
		return false;
	}, // submit

	getCurrentViewId: function() {
		return this.$el.find("li.active a").attr("page-name");
	}, // getCurrentViewId

	print: function() {
		printElement(this.getCurrentViewId());
		return false;
	}, // print

	updateDownloadLink: function() {
		if (!this.agreementView)
			return;

		var amount = parseInt(this.model.get("neededCash"), 10);
		var loanType = this.currentLoanTypeID;
		var repaymentPeriod = this.currentRepaymentPeriod;
		var view = this.getCurrentViewId();

		this.agreementView.$el.find('.download').attr('href', "" + window.gRootPath + "Customer/Agreement/Download?amount=" + amount + "&viewName=" + view + "&loanType=" + loanType + "&repaymentPeriod=" + repaymentPeriod);
        }, // updateDownloadLink

	createAgreementView: function(agreementdata) {
		var oViewArgs = {
			el: this.ui.agreement,
			onTabSwitch: _.bind(this.updateDownloadLink, this),
		};

		if (this.customer.get("CustomerPersonalInfo").TypeOfBusiness in [0, 4, 2])
			this.agreementView = new EzBob.Profile.ConsumersAgreementView(oViewArgs);
		else
			this.agreementView = new EzBob.Profile.CompaniesAgreementView(oViewArgs);

		this.agreementView.render(agreementdata);

		this.showSubmit();

		this.updateDownloadLink();
	}, // createAgreementView

	close: function() {
		clearInterval(this.timerId);
		this.model.off();
		ApplyForLoanView.__super__.close.call(this);
	}, // close
}); // EzBob.Profile.ApplyForLoanView
