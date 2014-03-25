var EzBob = EzBob || {};

EzBob.AddDirectorInfoView = Backbone.Marionette.ItemView.extend({
	template: '#add-director-info-template',

	region: {
		directorAddress: '.director_address'
	}, // region

	ui: {
		form: '.addDirectorInfoForm',
		addButton: '.addDirector'
	}, // ui

	events: {
		'click .directorBack': 'directorBack',
		'click .addDirector': 'directorAdd',
		'change   input': 'inputChanged',
		'click    input': 'inputChanged',
		'focusout input': 'inputChanged',
		'keyup    input': 'inputChanged',
		'change   select': 'inputChanged',
		'click    select': 'inputChanged',
		'focusout select': 'inputChanged',
		'keyup    select': 'inputChanged'
	}, // events

	addressModelChange: function() {
		return EzBob.App.trigger('dash-director-address-change', this.model);
	}, // addressModelChange

	onRender: function() {
		this.$el.find('.ezDateTime').splittedDateTime();
		this.$el.find('.alphaOnly').alphaOnly();
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.addressCaption').hide();

		this.validator = EzBob.validateAddDirectorForm(this.ui.form);

		var oAddressContainer = this.$el.find('#DirectorAddress');

		var that = this;

		this.addressView = new EzBob.AddressView({
			model: that.model.get('DirectorAddress'),
			name: 'DirectorAddress',
			max: 1,
			uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix')
		});

		this.model.get('DirectorAddress').on('all', function() {
			return that.trigger('director:addressChanged');
		});

		this.addressView.render().$el.appendTo(oAddressContainer);

		EzBob.Validation.addressErrorPlacement(this.addressView.$el, this.addressView.model);

		EzBob.UiAction.registerView(this);

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		return this;
	}, // onRender

	directorBack: function() {
		return EzBob.App.trigger('add-director-back');
	}, // directorBack

	directorAdd: function() {
		var enabled = this.validator.checkForm() && this.addressView.model.length > 0;

		this.ui.addButton.toggleClass('disabled', !enabled);

		if (!enabled)
			return false;

		var data = this.ui.form.serializeArray();

		BlockUi('on');

		var request = $.post(this.ui.form.attr('action'), data);

		request.done(function(res) {

			if (res.success)
				return EzBob.App.trigger('director-added');
			else {
				if (res.error)
					return EzBob.App.trigger('error', res.error);
				else
					return EzBob.App.trigger('error', 'Error occurred, try again');
			} // if
		});

		request.always(function() {
			return BlockUi('off');
		});

		return false;
	}, // directorAdd

	inputChanged: function() {
		var enabled = this.validator.checkForm() && this.addressView.model.length > 0;
		return this.ui.addButton.toggleClass('disabled', !enabled);
	}, // inputChanged
}); // EzBob.AddDirectorInfoView
