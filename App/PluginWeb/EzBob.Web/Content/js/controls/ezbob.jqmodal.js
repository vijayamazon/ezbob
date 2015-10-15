var EzBob = EzBob || {};

EzBob.JqModalRegion = Backbone.Marionette.Region.extend({
	initialize: function() {
		this.on('view:show', this.showModal, this);
		this.dialog = $('<div/>');
	}, // initialize

	isUnderwriter: document.location.href.indexOf("Underwriter") > -1,
	isWizard: document.location.href.indexOf("Wizard") > -1,

	el: 'fake',

	getEl: function(selector) {
		return this.dialog;
	}, // getEl

	showModal: function(view) {
		view.on('close', this.hideModal, this);

		var option = view.jqoptions();

		if (this.isUnderwriter) {
			option['resizable'] = true;
			option['draggable'] = true;
		} // if

		this.dialog.dialog(option);

		var self = this;
		this.dialog.on('dialogclose', function() { self.close(); });

		this.dialog.find('.ui-dialog').addClass('box');
		this.dialog.find('.ui-dialog-titlebar').addClass('box-title');
		this.dialog.parent('.ui-dialog').find('.ui-dialog-buttonset button').addClass('btn btn-primary');

		if (view.onAfterShow)
			view.onAfterShow.call(view);

		if (this.isWizard) {
			$('body').scrollTo(0);
		}
	}, // showModal

	hideModal: function() {
		this.dialog.dialog('destroy');
	}, // hideModal
}); // EzBob.JqModalRegion
