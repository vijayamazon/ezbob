EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.InstantOfferView = EzBob.Broker.SubmitView.extend({
	initialize: function() {
		EzBob.Broker.InstantOfferView.__super__.initialize.apply(this, arguments);
		this.$el = $('#section-dashboard-instant-offer');
		this.initSubmitBtn('button[type=submit]');
		this.initValidatorCfg();
	}, // initialize

	events: function() {
		var evt = EzBob.Broker.InstantOfferView.__super__.events.apply(this, arguments);
		return evt;
	}, // events

	onRender: function () {
		EzBob.Broker.InstantOfferView.__super__.onRender.apply(this, arguments);
		
		this.$el.find('#AnnualTurnover').moneyFormat();
		this.$el.find('#AnnualProfit').moneyFormat();
		this.$el.find('#NumOfEmployees').numericOnly(6);
	},
	clear: function() {
		EzBob.Broker.AddCustomerView.__super__.clear.apply(this, arguments);

		this.$el.find('input[type="text"],select').val('').blur();
		this.$el.find('input[type="checkbox"]').attr('checked', false);

		this.inputChanged();
	}, // clear

	setAuthOnRender: function() {
		return false;
	}, // setAuthOnRender

	onFocus: function() {
		EzBob.Broker.InstantOfferView.__super__.onFocus.apply(this, arguments);
		this.$el.find('#CompanyNameNumber').focus();
	}, // onFocus

	
	onSubmit: function (event) {
		var self = this;
		var oData = this.$el.find('form').serializeArray();
		
		oData = _.filter(oData, function(obj) {
			if (obj.name == "IsHomeOwner") {
				obj.value = self.$el.find('#IsHomeOwner').is(":checked");
			}
			
			if (obj.name == "AnnualTurnover" || obj.name == "AnnualProfit") {
				obj.value = self.$el.find('#' + obj.name).autoNumericGet();
			}
			
			return obj;
		});
		
		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/GetOffer', oData);
		
		oRequest.success(function(res) {
			UnBlockUi();
			console.log('res', res);
			if (res.success) {
				self.clear();
				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);

			self.setSubmitEnabled(true);
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to get instant offer. Please retry.');
		});
	}, // onSubmit

	initValidatorCfg: function() {
		this.validator = this.$el.find('form').validate({
			rules: {
				CompanyNameNumber: { required: true },
				AnnualTurnover: { required: true, defaultInvalidPounds: true, regex: '^(?!£ 0.00$)' },
				AnnualProfit: { required: true, defaultInvalidPounds: true, regex: '^(?!£ 0.00$)' },
				NumOfEmployees: { required: true, maxlength: 6, number: true, min: 1 },
				MainApplicantCreditScore: { required: true },
			},

			messages: {
				
			},

			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
		});
	}, // initValidatorCfg
}); // EzBob.Broker.AddCustomerView
