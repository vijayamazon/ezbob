EzBob = EzBob || {};

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

	$.get(window.gRootPath + 'Broker/BrokerHome/LoadStaticData', function(oResponse) {
		if (!oResponse.success)
			return;

		var data = oResponse.data;

		var oTerms = $('#broker-terms-and-conditions');
		oTerms.html(data.Terms);
		oTerms.attr('data-terms-version', data.TermsID);

		var oSmsCounts = $('#broker-sms-count');
		oSmsCounts.attr('data-max-per-number', data.MaxPerNumber);
		oSmsCounts.attr('data-max-per-page', data.MaxPerPage);

		if (oSmsCounts.attr('data-force-captcha') === 'yes')
			EzBob.App.trigger('brkr:signup-with-captcha');

		var oLinks = $('.marketing-files');
		var sTemplate = oLinks.attr('data-url-template');

		EzBob.CrmActions = data.Crm.CrmActions;
		EzBob.CrmStatuses = data.Crm.CrmStatuses;
		EzBob.CrmRanks = data.Crm.CrmRanks;

		_.each(data.Files, function(fd) {
			oLinks.append($('<li />').append(
				$('<a />').attr('href', sTemplate.replace('__FILE_ID__', fd.FileID)).text(fd.DisplayName)
			));
		});

		oLinks.removeAttr('data-url-template');
	});

	var sMsgOnStart = $('body').attr('data-msg-on-start');

	if (sMsgOnStart) {
		EzBob.App.trigger($('body').attr('data-msg-on-start-severity') || 'error', sMsgOnStart);

		$('body').removeAttr('data-msg-on-start');
		$('body').removeAttr('data-msg-on-start-severity');
	} // if
}); // document.ready
