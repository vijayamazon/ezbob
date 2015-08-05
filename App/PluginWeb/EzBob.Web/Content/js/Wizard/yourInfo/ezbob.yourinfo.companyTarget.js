var EzBob = EzBob || {};

EzBob.companyTargets = Backbone.View.extend({
	initialize: function (options) {
		this.template = _.template($('#CompanyTargets').html());
		this.jsonModel = options.model;
	}, // initialize

	events: {
		'dblclick .targets': 'targetsDoubleClicked',
		'click .targets': 'targetsClicked',
	}, // events

	render: function () {
		this.$el.html(this.template({ jsonModel: this.jsonModel }));

		this.targetsList = this.$el.find('ul.targets');

		var that = this;

		var screenWidth = $(document).width();

		if (screenWidth > 920)
			screenWidth = 920;

		this.$el.dialog({
			autoOpen: true,
			title: 'Select company',
			modal: true,
			resizable: true,
			width: screenWidth,
			height: 500,
			minHeight: 200,
			buttons: [
				{
					text: 'Cancel',
					'class': 'button btn-grey',
					click: function () { that.btnCancelClick(); },
					'ui-event-control-id': 'company-target:cancel',
				},
				{
					text: 'Skip',
					'class': 'button btn-green',
					click: function () { that.btnNotFoundClick(); },
					'ui-event-control-id': 'company-target:not-found',
				},
				{
					text: 'OK',
					'class': 'button btn-green btnTargetOk disabled',
					click: function () { that.btnOkClick(); },
					'ui-event-control-id': 'company-target:ok',
				},
			],
			close: function(evt, ui) { $('html, body').removeAttr('style'); },
		});

		var oWidget = this.$el.dialog('widget');

		oWidget.find('.ui-dialog-title').addClass('address-dialog-title');
		oWidget.find('.ui-dialog-titlebar').addClass('address-dialog-titlebar');
		oWidget.find('.ui-dialog-buttonpane').addClass('address-dialog-buttonpane');

		EzBob.UiAction.registerView(this);

		this.$el.find('.targets').beautifullList();

		$('html, body').css({
			'overflow': 'hidden',
			'height': '100%'
		});

		return this;
	}, // render

	targetsClicked: function (evt) {
		EzBob.UiAction.saveOne(EzBob.UiAction.evtClick(), evt.target);
		$('.btnTargetOk').removeClass('disabled');
	}, // targetsClicked

	targetsDoubleClicked: function (evt) {
		this.targetsClicked(evt);
		EzBob.UiAction.saveOne(EzBob.UiAction.evtLinked(), evt.target);
		this.btnOkClick();
	}, // targetsDoubleClicked

	btnOkClick: function () {
		if (this.targetsList.attr('data') == null)
			this.targetsList.css("border", "1px solid red");
		else {
			$('html, body').removeAttr('style');
			this.trigger('BusRefNumGot', this.jsonModel[this.targetsList.attr('data')]);
			this.btnCancelClick();
		} // if
	}, // btnOkClick

	btnNotFoundClick: function () {
		$('html, body').removeAttr('style');
		this.trigger('BusRefNumGot', null);
		this.btnCancelClick();
	}, // btnNotFoundClick

	btnCancelClick: function () {
		$('html, body').removeAttr('style');
		this.$el.dialog('close');
	}, // btnCancelClick
}); // EzBob.companyTargets
