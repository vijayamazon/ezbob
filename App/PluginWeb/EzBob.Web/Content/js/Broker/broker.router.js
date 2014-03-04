EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.Router = Backbone.Router.extend({
	routes: {
		'': 'signup',
		'signup': 'signup',
		'login': 'login',
		'dashboard': 'dashboard',
		'forgotten': 'forgotten',
		'add': 'addCustomer',
		'customer/:customerid': 'showCustomer',
		'*z': 'forbidden',
	}, // routes

	initialize: function() {
		this.setAuth($('body').attr('data-auth'));
		$('body').removeAttr('data-auth');

		$('#user-menu').hide().removeClass('hide');
	}, // initialize

	getAuth: function() {
		return this.authData;
	}, // getAuth

	setAuth: function(sAuth) {
		this.authData = sAuth || '';
		$('#user-menu .log-off').tooltip({ placement: 'bottom', title: this.authData }).tooltip("enable").tooltip('fixTitle');
	}, // setAuth

	logoff: function() {
		if (this.views.dashboard)
			this.views.dashboard.clear();

		$.post('' + window.gRootPath + 'Broker/BrokerHome/Logoff?sContactEmail=' + this.getAuth());
		this.setAuth();
		this.login();
	}, // logoff

	signup: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.showDashboard();
		else {
			this.createView('signup', EzBob.Broker.SignupView);
			this.show('signup', 'log-in', 'signup');
		} // if
	}, // signup

	login: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.showDashboard();
		else {
			this.createView('login', EzBob.Broker.LoginView);
			this.show('login', 'sign-up', 'login');
		} // if
	}, // login

	forgotten: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.showDashboard();
		else {
			this.createView('forgotten', EzBob.Broker.ForgottenView);
			this.show('forgotten', 'sign-up', 'forgotten');
		} // if
	}, // forgotten

	dashboard: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.showDashboard();
		else
			this.login();
	}, // dashboard

	showDashboard: function() {
		this.createView('dashboard', EzBob.Broker.DashboardView);
		this.show('dashboard', 'log-off', 'dashboard');
	}, // showDashboard

	addCustomer: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth()){
			this.createView('addCustomer', EzBob.Broker.AddCustomerView);
			this.show('add-customer', 'log-off', 'addCustomer');
		}
		else
			this.login();
	}, // addCustomer

	showCustomer: function(customerid) {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth()) {
			if (this.views)
				this.views.customer = null;

			this.createView('customer', EzBob.Broker.CustomerDetailsView, { customerid: customerid });
			this.show('customer-details', 'log-off', 'customer');
		}
		else
			this.login();
	}, // showCustomer

	forbidden: function() {
		this.show(this.forbiddenSection());
	}, // forbidden

	isForbidden: function() {
		return '-' === this.getAuth();
	}, // isForbidden

	show: function(sSectionName, sButtonClass, sViewName) {
		sSectionName = sSectionName.toLowerCase();
		var oSection;

		if (sSectionName === this.forbiddenSection())
			oSection = $('.section-' + sSectionName);
		else {
			if (this.isForbidden())
				sSectionName = this.forbiddenSection();

			oSection = $('.section-' + sSectionName);

			if (!oSection.length) {
				sSectionName = this.forbiddenSection();
				oSection = $('.section-' + sSectionName);
			} // if
		} // if

		if (sSectionName === this.forbiddenSection())
			sButtonClass = null;

		var self = this;

		var oShowPage = function() {
			var oView = null;

			if (sViewName && self.views && self.views[sViewName]) {
				oView = self.views[sViewName];

				if (oView.setSidebar)
					oView.setSidebar($('.common-customer-sidebar'));
				
				oView.render();
			}
			else
				$('#hidden-container').append($('.common-customer-sidebar'));

			oSection.fadeIn(400, function() {
				if (oView)
					oView.onFocus();
			});
		}; // oShowPage

		var oActive = $('.page-section:visible');

		if (oActive.length)
			oActive.fadeOut(400, oShowPage);
		else
			oShowPage();

		var oMenu = $('#user-menu');

		if (sButtonClass) {
			oMenu.show();
			$('.menu-btn', oMenu).hide();
			$('.' + sButtonClass, oMenu).show();
		}
		else {
			oMenu.hide();
			$('.menu-btn', oMenu).hide();
		} // if
	}, // show

	forbiddenSection: function() { return 'forbidden'; }, // forbiddenSection

	createView: function(sViewName, oViewType, oViewTypeArgs) {
		if (!this.views)
			this.views = {};

		if (!this.views[sViewName]) {
			var oArgs = $.extend({}, oViewTypeArgs || {}, { router: this, });
			this.views[sViewName] = new oViewType(oArgs);
		} // if
	}, // createView
}); // EzBob.Broker.Router
