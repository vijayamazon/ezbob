/// <reference path="~/Content/js/App/ezbob.clicktale.js" />
/// <reference path="../../ezbob.design.js" />
/// <reference path="~/Content/js/App/ezbob.app.js" />

var EzBob = EzBob || {};

EzBob.ThankYouWizardPage = Backbone.View.extend({
	initialize: function () {
		this.template = $(_.template($("#lastWizardThankYouPage").html(), { ordty: ordty }));
		this.readyToProceed = true;
	}, // initialize

	render: function () {
		$.getJSON(window.gRootPath + "Customer/Wizard/EarnedPointsStr").done(function (data) {
			if (data.EarnedPointsStr)
				$('#EarnedPoints').text(data.EarnedPointsStr);
		}); // done

		this.$el.html(this.template);
		$('.sidebarBox').find('li[rel]').setPopover('left');

		if (!this.model.get('IsOffline'))
			this.$el.find('.offline').remove();
		else
			this.$el.find('.notoffline').remove();

		this.readyToProceed = true;
		return this;
	} // render
});
