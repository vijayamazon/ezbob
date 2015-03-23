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
			mainPageClass: '#wrapper,footer',
		});

		this.router = new EzBob.Profile.ProfileRouter(this.customer);
		this.router.on("details", this.loanDetails, this);
		this.router.on("getCash", this.getCash, this);
		this.router.on("payEarly", this.makePayment, this);
		this.router.on("menuWidgetShown", this.menuWidgetShown, this);
	},

	render: function() {
		this.profileMain.show();

		this.getCashView.render();
		this.payEarlyView.render();
		this.signWidget.render();
		this.processingMessageView.render();

		this.payEarlyView.$el.appendTo(this.widgetsContainer);
		this.getCashView.$el.appendTo(this.widgetsContainer);
		this.signWidget.$el.appendTo(this.signContainer);
		this.processingMessageView.$el.appendTo(this.processingMessageContainer);

		EzBob.UiAction.registerView(this);
		EzBob.UiAction.registerChildren(this.profileMain);

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
		$(document).attr("title", "Get Cash: Select Loan Amount | EZBOB");
		EzBob.App.GA.trackPage('/Customer/Profile/GetCash');

		var applyForLoanView = new EzBob.Profile.ApplyForLoanTopView({ customer: this.customer, model: new EzBob.Profile.ApplyForLoanTopViewModel() });

		applyForLoanView.on('back', this.applyForLoanBack, this);
		applyForLoanView.on('submit', this.applyForLoanSubmit, this);

		this.getCashRegion.show(applyForLoanView);
		this.hideProfile();
		this.marketing("GetCash");
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
		$(document).attr("title", "Dashboard: User Dashboard | EZBOB");
		this.getCashRegion.close();
		this.showProfile();
		scrollTop();
		this.marketing("Dashboard");
	},

	applyForLoanBack: function() {
		this.router.navigate("");
		this.menuWidgetShown();
	},

	payEarlyBack: function() {
		this.router.previous();
	},

	applyForLoanSubmit: function(creditSum) {
		this.applyForLoanBack();
		this.getCashModel.set('availableCredit', this.getCashModel.get('availableCredit') - creditSum);
	},

	makePayment: function(id) {
		var title = (this.customer.get("hasLateLoans") ? "Pay Late:" : "Pay Early:") + " User Payment | EZBOB";
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
