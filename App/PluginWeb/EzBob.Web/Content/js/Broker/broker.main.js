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

	$.getJSON(window.gRootPath + 'Broker/BrokerHome/LoadStaticData', {}, function(oResponse) {
		if (!oResponse.success)
			return;

		$('#crm-lookups .crm-lookup').load_display_value({
			data_source: oResponse,
			set_text: true,
			callback: function(sName, oValue) { return JSON.stringify(oValue); },
		});

		var oTerms = $('#broker-terms-and-conditions');
		oTerms.html(oResponse.broker_terms.text);
		oTerms.attr('data-terms-version', oResponse.broker_terms.id);

		var oSmsCounts = $('#broker-sms-count');
		oSmsCounts.attr('data-max-per-number', oResponse.max_per_number);
		oSmsCounts.attr('data-max-per-page', oResponse.max_per_page);

		var oLinks = $('.marketing-files');
		var sTemplate = oLinks.attr('data-url-template');

		_.each(oResponse.marketing_files, function(fd) {
			oLinks.append($('<li />').append(
				$('<a />').attr('href', sTemplate.replace('__FILE_ID__', fd.FileID)).text(fd.DisplayName)
			));
		});

		oLinks.removeAttr('data-url-template').removeClass('marketing-files');
	});

	var sMsgOnStart = $('body').attr('data-msg-on-start');

	if (sMsgOnStart) {
		EzBob.App.trigger($('body').attr('data-msg-on-start-severity') || 'error', sMsgOnStart);

		$('body').removeAttr('data-msg-on-start');
		$('body').removeAttr('data-msg-on-start-severity');
	} // if
}); // document.ready
