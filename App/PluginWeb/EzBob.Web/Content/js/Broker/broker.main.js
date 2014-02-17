﻿EzBob = EzBob || {};

$(document).ready(function() {
	EzBob.Config = $.extend({}, EzBob.Config, EzBob.LoadedConfig);

	$('.page-section').hide().removeClass('hide');

	var notifications = new EzBob.NotificationsView({ el: $('.notifications') });

	var oRouter = new EzBob.Broker.Router();
	Backbone.history.start();

	if (oRouter.isForbidden()) {
		oRouter.forbidden();
		return;
	} // if

	$('#user-menu .menu-btn').click(function(event) {
		event.preventDefault();
		event.stopPropagation();

		var o = $(event.currentTarget);

		if (o.hasClass('log-in'))
			oRouter.login();
		else if (o.hasClass('sign-up'))
			oRouter.signup();
		else
			oRouter.logoff();
	});

	var oFieldStatusIcons = $('IMG.field_status');
	oFieldStatusIcons.filter('.required').field_status({ required: true });
	oFieldStatusIcons.not('.required').field_status({ required: false });
}); // document.ready
