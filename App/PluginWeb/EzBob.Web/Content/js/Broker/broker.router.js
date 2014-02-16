EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.Router = Backbone.Router.extend({
	routes: {
		'': 'signup',
		'signup': 'signup',
		'login': 'login',
		'dashboard': 'dashboard',
		'forgotten': 'forgotten',
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
		$.post('' + window.gRootPath + 'Broker/BrokerHome/Logoff');
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

		this.navigate(sSectionName);

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

	createView: function(sViewName, oViewType) {
		if (!this.views)
			this.views = {};

		if (!this.views[sViewName])
			this.views[sViewName] = new oViewType({ router: this, });
	}, // createView
}); // EzBob.Broker.Router
