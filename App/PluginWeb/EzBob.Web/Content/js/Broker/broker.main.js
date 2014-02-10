$(document).ready(function() {
	var oRouter = new EzBob.Broker.Router();
	Backbone.history.start();

	$('#user-menu .menu-btn').click(function() {
		event.preventDefault();
		event.stopPropagation();

		var o = $(event.currentTarget);

		if (o.hasClass('log-in'))
			oRouter.login();
		else if (o.hasClass('sign-up'))
			oRouter.signup();
		else
			window.location = 'http://www.ezbob.com';
	});

	var sAuth = $('body').attr('data-auth');

	if (sAuth === '-') {
		oRouter.forbidden();
		return;
	} // if

	var oFieldStatusIcons = $('IMG.field_status');
	oFieldStatusIcons.filter('.required').field_status({ required: true });
	oFieldStatusIcons.not('.required').field_status({ required: false });
}); // document.ready
