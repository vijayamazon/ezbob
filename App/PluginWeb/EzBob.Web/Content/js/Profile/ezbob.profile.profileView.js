var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ProfileView = Backbone.View.extend({
	initialize: function(options) {
		$('#profile-main-to-be-replaced').html(
			$(_.template($("#profile-main-template").html(), {}))
		);

		this.profileMain = $('#profile-main');

		this.customer = options;
		this.payEarlyView = new EzBob.Profile.PayEarlyView({ model: options });

		this.getCashModel = new EzBob.Profile.GetCashModel({ customer: options });
		this.getCashView = new EzBob.Profile.GetCashView({ model: this.getCashModel, customer: options });
		this.getCashView.on("turnover", this.turnover, this);
		this.processingPopup = new EzBob.Profile.ProccessingAutomationPopupView({ model: this.getCashModel });

		this.signWidget = new EzBob.Profile.SignWidget({ customerModel: options });
		this.signWidget.on('payEarly', this.makePayment, this);

		this.processingMessageView = new EzBob.Profile.ProccessingMessageView({ model: options });
		this.processingMessageContainer = $('.proccessing-message').empty();

		this.widgetsContainer = $('.d-widgets').empty();

		this.getCashRegion = new Backbone.Marionette.Region({ el: "#get-cash" });

		this.signContainer = $("#message-sign");

		this.processingMessageView.on('getCash', this.getCash, this);
		this.processingMessageView.on('payEarly', this.makePayment, this);

		this.scratchView = EzBob.ScratchCards.Select(this.customer.get('LotteryCode'), {
			customerID: this.customer.get('Id'),
			playerID: this.customer.get('LotteryPlayerID'),
			customerMode: true,
			mainPageClass: '#wrapper',
		});

		this.router = new EzBob.Profile.ProfileRouter(this.customer);
		this.router.on("details", this.loanDetails, this);
		this.router.on("getCash", this.getCash, this);
		this.router.on("payEarly", this.makePayment, this);
		
		this.router.on("menuWidgetShown", this.menuWidgetShown, this);
	},

	render: function() {
		this.profileMain.show();
		
		this.payEarlyView.render();
		this.signWidget.render();
		this.processingMessageView.render();

		this.payEarlyView.$el.appendTo(this.widgetsContainer);
		this.getCashView.$el.appendTo(this.widgetsContainer);
		this.getCashView.render();
		this.signWidget.$el.appendTo(this.signContainer);
		this.processingMessageView.$el.appendTo(this.processingMessageContainer);

		EzBob.UiAction.registerView(this);
		EzBob.UiAction.registerChildren(this.profileMain);

		if (this.customer.isFinishedWizard) {
			this.processingPopup.render();
			this.finishedWizard();
			this.customer.isFinishedWizard = false;
		}

		/*
		if (this.customer.get('state') == 'get' && !this.customer.get('hasLoans'))
			window.location.href = "#GetCash";
		*/
		if (this.customer.get('hasLoans'))
			window.location.href = "#AccountActivity";

		if (this.customer.get('IsWhiteLabel')) {
			$('.header-info').show();
			$('.header-info-text').html('<div class="whiteLabel"></div>');
		} else {
			$('.header-info').show();
			$('.header-info-text').text('MY ACCOUNT');
		}

		$('footer.location-customer-everline .privacy-and-cookie-policy').hide();

		return this;
	},

	loanDetails: function(id) {
		EzBob.App.GA.trackPage('/Customer/Profile/LoanDetails');

		var loan = new EzBob.Profile.LoanModel();
		loan.loanId = id;
		loan.fetch();

		var loanDetailView = new EzBob.Profile.LoanDetailsView({ model: loan, customer: this.customer });
		loanDetailView.render();

		this.hideProfile();
		this.getCashRegion.show(loanDetailView);
		this.marketing("LoanDetails");
	},

	getCash: function() {
		$(document).attr("title", "Get Cash: Select Loan Amount");

		EzBob.App.GA.trackPage('/Customer/Profile/GetCash', 'Get Cash: Select Loan Amount', this.getGTMVariables());

		var applyForLoanView = new EzBob.Profile.ApplyForLoanTopView({ customer: this.customer, model: new EzBob.Profile.ApplyForLoanTopViewModel() });

		applyForLoanView.on('back', this.applyForLoanBack, this);
		applyForLoanView.on('submit', this.applyForLoanSubmit, this);

		this.getCashRegion.show(applyForLoanView);
		this.hideProfile();
		this.marketing("GetCash");
	},

	turnover: function () {
		$(document).attr("title", "Update turnover");

		EzBob.App.GA.trackPage('/Customer/Profile/Turover', 'Update turnover', this.getGTMVariables());

		var turnoverView = new EzBob.Profile.TurnoverView({ customer: this.customer });

		turnoverView.on('cancel', this.turnoverCancel, this);
		turnoverView.on('next', this.turnoverNext, this);

		this.getCashRegion.show(turnoverView);
		this.hideProfile();
		this.marketing("GetCash");
	},

	finishedWizard: function () {
		dataLayer.push({ 'event': 'finished-wizard' });
		EzBob.App.GA.trackPage('/Customer/Profile/FinishedWizard', 'Profile: Finished Wizard', this.getGTMVariables());
	}, //finishedWizard

	getGTMVariables: function(){
		var address = this.customer.get('PersonalAddress');
		var postcode = '';
		if (address && address.models && address.models.length > 0) {
			postcode = this.customer.get('PersonalAddress').models[0].get('Postcode') || '';
		}

		var personalInfo = this.customer.get('CustomerPersonalInfo');

		var varaibles = {
			Amount: this.customer.get('CreditSum'),
			Length: this.customer.get('LastApprovedRepaymentPeriod'),
			Gender: personalInfo ? personalInfo.GenderName || '' : '',
			Age: personalInfo ? personalInfo.Age || '' : '',
			Postcode: postcode,
			TypeofBusiness: personalInfo ? personalInfo.TypeOfBusinessDescription || '' : '',
			IndustryType: personalInfo ? personalInfo.IndustryTypeDescription || '' : '',
			LeadID: this.customer.get('RefNumber') || '',
			AccountStatus: this.customer.get('Status')
		};

		return varaibles;
	},
	showProfile: function() {
		this.profileMain.show();

		if (this.scratchView)
			this.scratchView.render();
	}, // showProfile

	hideProfile: function() {
		this.profileMain.hide();

		if (this.scratchView)
			this.scratchView.hide();
	}, // hideProfile

	menuWidgetShown: function() {
		$(document).attr("title", "Dashboard: User Dashboard");
		this.getCashRegion.close();
		this.showProfile();
		scrollTop();
		this.marketing("Dashboard");
	},

	applyForLoanBack: function() {
		this.router.navigate("");
		this.getCashModel.refresh();
		this.menuWidgetShown();
	},

	payEarlyBack: function() {
		this.router.previous();
	},

	applyForLoanSubmit: function(creditSum) {
		this.applyForLoanBack();
		this.getCashModel.set('availableCredit', this.getCashModel.get('availableCredit') - creditSum);
	},

	turnoverCancel: function () {
		this.router.navigate("");
		this.getCashModel.refresh();
		this.menuWidgetShown();
	},

	turnoverNext: function () {
		this.turnoverCancel();
		this.getCashView.doApplyForALoan();
	},

	makePayment: function(id) {
		var title = (this.customer.get("hasLateLoans") ? "Pay Late:" : "Pay Early:") + " User Payment";
		$(document).attr("title", title);

		EzBob.App.GA.trackPage('/Customer/Profile/MakePayment');

		var payEarlyView = new EzBob.Profile.MakeEarlyPayment({ el: this.payEarlyDiv, customerModel: this.customer, loanId: id });
		payEarlyView.on('submit back', this.payEarlyBack, this);

		this.getCashRegion.show(payEarlyView);
		this.hideProfile();
		this.marketing("MakePayment");
	},


	marketing: function(page) {
		var marketing;
		var isDashboard = true;
		switch (page) {
			case "Dashboard":
				marketing = EzBob.dbStrings.MarketingDashboard;
				break;
			case "MakePayment":
				marketing = EzBob.dbStrings.MarketingDashboardMakePayment;
				break;
			case "GetCash":
				marketing = EzBob.dbStrings.MarketingDashboardGetCash;
				break;
			case "LoanDetails":
				marketing = EzBob.dbStrings.MarketingDashboardLoanDetails;
				break;
			default:
				isDashboard = false;
				marketing = EzBob.dbStrings.MarketingDefault;
				break;
		}
		if (isDashboard && marketing) {
			$("#defaultMarketing").hide();
			$("#marketingProggress").show().html(marketing);
		} else {
			$("#defaultMarketing").show();
			$("#marketingProggress").hide().html(marketing);
		}
	}
});
