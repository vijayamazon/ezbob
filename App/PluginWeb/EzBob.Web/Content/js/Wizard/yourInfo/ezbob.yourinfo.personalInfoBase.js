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

	addressErrorPlacement: function(el, model) {
		var $el = $(el);

		$el.on('focusout', function() {
			if (model.length === 0) {
				var oButton = $el.find('.addAddress');

				oButton.tooltip({ title: 'Please lookup your post code' });

				$el.hover(
					function() { oButton.tooltip('show'); },
					function() { oButton.tooltip('hide'); }
				); // on hover
			} // if
		}); // on focus out

		model.on('change', function() {
			if (model.length > 0)
				$el.find('.addAddress').tooltip('destroy');
		}); // on model changed
	}, // addressErrorPlacement

	clearAddressError: function(el) {
		EzBob.Validation.unhighlight(this.$el.find(el));
		EzBob.Validation.unhighlightFS(this.$el.find(el));
	}, // clearAddressError
});