EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.Router = Backbone.Router.extend({
	routes: {
		'': 'signup',
		'signup': 'signup',
		'login': 'login',
		'*z': 'forbidden',
	},

	signup: function() {
		this.show('signup', 'log-in');
	}, // signup

	login: function() {
		this.show('login', 'sign-up');
	}, // login

	forbidden: function() {
		this.show(this.forbiddenSection());
	}, // forbidden

	show: function(sSectionName, sButtonClass) {
		sSectionName = sSectionName.toLowerCase();
		var oSection;

		if (sSectionName === this.forbiddenSection())
			oSection = $('.section-' + sSectionName);
		else {
			var sAuth = $('body').attr('data-auth');

			if (sAuth === '-')
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
}); // EzBob.Broker.Router
