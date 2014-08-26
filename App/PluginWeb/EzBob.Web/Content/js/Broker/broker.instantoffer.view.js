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

		this.targetBusiness(oData);
	}, // onSubmit
	
	targetBusiness: function (oData) {
		var self = this;
		$.get(window.gRootPath + "Broker/BrokerHome/TargetBusiness", { companyNameNumber: this.$el.find('#CompanyNameNumber').val() })
			.success(function (reqData) {
				if (reqData == undefined || reqData.success === false)
					self.getInstantOffer(oData, { BusRefNum: "NotFound" });
				else {
					switch (reqData.length) {
						case 0:
							EzBob.ShowMessage("Company was not found", "Warning", null, "OK");
							self.getInstantOffer(oData, { BusRefNum: "NotFound" });
							break;
						case 1:
							self.getInstantOffer(oData, reqData[0]);
							break;
						default:
							var companyTargets = new EzBob.companyTargets({ model: reqData });
							companyTargets.render();
							companyTargets.on("BusRefNumGetted", function (targetingData) {
								self.getInstantOffer(oData, targetingData);
							});
							break;
					} // switch
				} // if
			}).always(function () {
				UnBlockUi();
			});
	},
	
	getInstantOffer: function(oData, companyData) {
		console.log('companyData', oData, companyData);
		var self = this;

		if (companyData && companyData.BusRefNum != 'skip') {
			oData.push({ name: "PostCode", value: companyData.PostCode });
			oData.push({ name: "BusName", value: companyData.BusName });
			oData.push({ name: "BusRefNum", value: companyData.BusRefNum });
			oData.push({ name: "LegalStatus", value: companyData.LegalStatus });
		} else {
			oData.push({ name: "BusRefNum", value: "NotFound" });
		}

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/GetOffer', oData );

		oRequest.success(function (res) {
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

		oRequest.fail(function () {
			UnBlockUi();
			self.setSubmitEnabled(true);
			EzBob.App.trigger('error', 'Failed to get instant offer. Please retry.');
		});
	},

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
