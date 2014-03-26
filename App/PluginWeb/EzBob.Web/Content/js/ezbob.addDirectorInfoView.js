var EzBob = EzBob || {};

EzBob.AddDirectorInfoView = EzBob.ItemView.extend({
	template: '#add-director-info-template',

	initialize: function(options) {
		this.backButtonCaption = options.backButtonCaption || 'Back';
	}, // initialize

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

		'change   input': 'canSubmit',
		'click    input': 'canSubmit',
		'focusout input': 'canSubmit',
		'keyup    input': 'canSubmit',

		'change   select': 'canSubmit',
		'click    select': 'canSubmit',
		'focusout select': 'canSubmit',
		'keyup    select': 'canSubmit',
	}, // events

	addressModelChange: function() {
		return EzBob.App.trigger('dash-director-address-change', this.model);
	}, // addressModelChange

	setCustomerID: function(nCustomerID) {
		this.$el.find('#nCustomerID').val(nCustomerID);
	}, // setCustomerID

	onRender: function() {
		this.$el.find('.directorBack').text(this.backButtonCaption);
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

		this.canSubmit();

		return this;
	}, // onRender

	directorBack: function() {
		this.trigger(this.backEvtName());
	}, // directorBack

	directorAdd: function() {
		if (!this.canSubmit())
			return false;

		var data = this.ui.form.serializeArray();

		BlockUi('on');

		var request = $.post(this.ui.form.attr('action'), data);

		var self = this;

		request.done(function(res) {
			if (res.success)
				self.trigger(self.successEvtName());
			else {
				if (res.error)
					EzBob.App.trigger('error', res.error);
				else
					EzBob.App.trigger('error', 'Error occurred, try again');

				self.trigger(self.failEvtName());
			} // if
		}); // on success

		request.fail(function() {
			self.trigger(self.failEvtName());
		}); // on fail

		request.always(function() {
			BlockUi('off');
		}); // always

		return false;
	}, // directorAdd

	canSubmit: function() {
		var bEnabled = this.validator.checkForm() && (this.addressView.model.length > 0);
		this.setSomethingEnabled(this.ui.addButton, bEnabled);
		return bEnabled;
	}, // canSubmit

	backEvtName: function() { return 'go-back'; }, // backEvtName
	successEvtName: function() { return 'success'; }, // successEvtName
	failEvtName: function() { return 'fail'; }, // failEvtName

	setBackHandler: function(oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.backEvtName(), oHandler);
	}, // setBackHandler

	setSuccessHandler: function(oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.successEvtName(), oHandler);
	}, // setSuccessHandler

	setFailHandler: function(oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.failEvtName(), oHandler);
	}, // setFailHandler
}); // EzBob.AddDirectorInfoView
