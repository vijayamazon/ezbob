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
	},

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
			this.show('dashboard', 'log-off');
		else {
			this.show('signup', 'log-in');
			this.createView('signup', EzBob.Broker.SignupView);
			this.views.signup.render();
		} // if
	}, // signup

	login: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.show('dashboard', 'log-off');
		else {
			this.show('login', 'sign-up');
			this.createView('login', EzBob.Broker.LoginView);
			this.views.login.render();
		} // if
	}, // login

	forgotten: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.show('dashboard', 'log-off');
		else {
			this.show('forgotten', 'sign-up');
			// this.createView('forgotten', EzBob.Broker.ForgottenView);
			// this.views.forgotten.render();
		} // if
	}, // forgotten

	dashboard: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.show('dashboard', 'log-off');
		else
			this.login();
	}, // dashboard

	forbidden: function() {
		this.show(this.forbiddenSection());
	}, // forbidden

	isForbidden: function() {
		return '-' === this.getAuth();
	}, // isForbidden

	getAuth: function() {
		return $('body').attr('data-auth');
	}, // getAuth

	setAuth: function(sAuth) {
		$('body').attr('data-auth', sAuth || '');
	}, // setAuth

	show: function(sSectionName, sButtonClass) {
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

		$('.page-section').addClass('hide');
		oSection.removeClass('hide');

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
