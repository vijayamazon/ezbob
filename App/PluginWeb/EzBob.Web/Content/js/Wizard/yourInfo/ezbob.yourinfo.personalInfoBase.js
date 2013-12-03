var EzBob = EzBob || {};

EzBob.YourInformationStepViewBase = Backbone.View.extend({
	initialize: function() {
		this.ViewName = '';
		this.delegateEvents();
	}, // initialize

	render: function() {
		this.$el.html(this.template(this.model.toJSON()));
		this.form = this.$el.find('form');

		if (!this.model.get('IsOffline'))
			this.$el.find('.offline').remove();
		else
			this.$el.find('.notoffline').remove();

		this.validator = this.getValidator()(this.form);

		this.$el.find('.ezDateTime').splittedDateTime();
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.alphaOnly').alphaOnly();

		this.$el.find('li[rel]').setPopover('left');

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		fixSelectValidate(this.$el.find('select'));

		return this;
	}, // render

	events: {
		'click .btn-continue': 'next'
	}, // events

	clearAddressError: function(el) {
		EzBob.Validation.unhighlight(this.$el.find(el));
		EzBob.Validation.unhighlightFS(this.$el.find(el));
	}, // clearAddressError
});