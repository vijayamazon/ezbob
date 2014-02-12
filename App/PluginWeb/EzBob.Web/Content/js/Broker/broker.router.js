EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.Router = Backbone.Router.extend({
	routes: {
		'': 'signup',
		'signup': 'signup',
		'login': 'login',
		'dashboard': 'dashboard',
		'*z': 'forbidden',
	},

	logoff: function() {
		$.post('' + window.gRootPath + 'Broker/BrokerHome/Logoff');
		window.location = 'http://www.ezbob.com';
	}, // logoff

	signup: function() {
		this.show('signup', 'log-in');
		this.createView('signup', EzBob.Broker.SignupView);
		this.views.signup.render();
	}, // signup

	login: function() {
		this.show('login', 'sign-up');
	}, // login

	dashboard: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.show('dashboard', 'log-off');
		else
			this.signup();
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
