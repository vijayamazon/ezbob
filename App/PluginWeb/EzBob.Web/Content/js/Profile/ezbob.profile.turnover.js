var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.TurnoverView = Backbone.Marionette.ItemView.extend({
	template: "#turnover-template",

	initialize: function(options) {
		this.customer = options.customer;
		return this;
	}, // initialize

	events: {
		'click #turnoverCancel': 'cancel',
		'click #turnoverNext': 'next',
		'change input': 'inputChanged'
	}, //events

	ui:{
		turnoverField: '#turnover',
		form: '#turnoverForm',
		turnoverNext: '#turnoverNext'

	}, //ui

	onRender: function () {
		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.validator = this.ui.form.validate({
			rules: { 
				turnover : { required: true , defaultInvalidPounds: true, regex: "^(?!£0.00$)", autonumericMin: 1, autonumericMax: 1000000000 }
			},
			messages: {
				turnover : { defaultInvalidPounds: "This field is required", regex: "This field is required" }
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
			ignore: ':not(:visible)',
		});

		this.$el.find('.cashInput').moneyFormat();
		EzBob.UiAction.registerView(this);
		return this;
	}, // onRender

	serializeData: function() {
		return {
			turnover: this.customer.get('Turnover'),
		};
	}, // serializeData

	inputChanged: function () {
		this.ui.turnoverNext.toggleClass('disabled', !this.validator.checkForm());
	}, // inputChanged

	cancel: function () {
		this.trigger('cancel');
		return false;
	}, //cancel

	next: function () {
		if (!this.validator.checkForm()) {
			return false;
		}

		var self = this;

		var turnover = this.ui.turnoverField.autoNumeric('get');
		BlockUi();
		$.post(window.gRootPath + 'Customer/Profile/UpdateTurnover', { turnover: turnover })
			.done(function () {
				self.trigger('next');
				self.customer.set('Turnover', turnover);
				self.customer.set('IsTurnoverExpired', false);
			})
			.error(function () {

			})
			.always(function () {
				UnBlockUi();
			});
		return false;
	} //next
}); // EzBob.Profile.TurnoverView
