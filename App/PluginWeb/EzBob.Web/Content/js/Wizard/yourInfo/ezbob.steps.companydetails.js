var EzBob = EzBob || {};

EzBob.CompanyDetailsStepModel = EzBob.WizardStepModel.extend({});

EzBob.CompanyDetailsStepView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#company-data-template').html());

		this.companyTypes = {
			entrepreneur: {
				View: null,
				Type: 'entrepreneur',
			},

			soletrader: {
				View: EzBob.NonLimitedInformationView,
				Type: 'NonLimited',
			},

			llp: {
				View: EzBob.LimitedInformationView,
				Type: 'Limited',
			},
		}; // companyTypes
		this.targetingTries = 0;
		this.validatorRules = this.ownValidationRules();
		this.validatorMessages = this.ownValidationRules();

		for (var ct in this.companyTypes) {
			var oView = this.companyTypes[ct].View;

			if (!oView)
				continue;

			this.validatorRules = _.extend({}, this.validatorRules, oView.prototype.ownValidationRules());
			this.validatorMessages = _.extend({}, this.validatorMessages, oView.prototype.ownValidationMessages());
		} // for

		this.companyTypes.pship3p = this.companyTypes.soletrader;
		this.companyTypes.pship = this.companyTypes.soletrader;
		this.companyTypes.limited = this.companyTypes.llp;

		this.events = _.extend({}, this.events, {
			'submit .CompanyDetailForm': 'submitForm',

			'click .btn-continue': 'next',

			'click .oobts': 'businessTypeSelected',

			'focus #OverallTurnOver': 'overallTurnOverFocus',
			'focus #WebSiteTurnOver': 'webSiteTurnOverFocus',

			'change   input': 'inputChanged',
			'click    input': 'inputChanged',
			'focusout input': 'inputChanged',
			'keyup    input': 'inputChanged',

			'change   select': 'inputChanged',
			'click    select': 'inputChanged',
			'focusout select': 'inputChanged',
			'keyup    select': 'inputChanged',
		}); // events

		this.validator = null;

		this.readyToProceed = false;
		this.curOobts = null;
	}, // initialize

	submitForm: function(evt) {
		evt.preventDefault();
		evt.stopPropagation();

		this.inputChanged();
		this.next();

		return false;
	}, // submitForm

	businessTypeSelected: function(evt) {
		// this.$el.find('.after-business-type *').enable();

		this.$el.find('.oobts').removeClass('oobts-selected');

		this.$el.find('.oobts i').removeClass('fa-square-o fa-check-square-o').addClass('fa-square-o');

		var oBtn = $(evt.currentTarget);

		oBtn.addClass('oobts-selected').find('i').removeClass('fa-square-o').addClass('fa-check-square-o');

		this.curOobts = oBtn.attr('data-oobts');

		var sEmail = this.model.get('Email');
		if (sEmail && sEmail.match && sEmail.match(/@ezbob\.com$/)) {
			this.$el.find('.oobts-common').removeClass('oobts-common').addClass('oobts-test-user');

			var self = this;

			window.setTimeout(function() {
				self.$el.find('.oobts-test-user').removeClass('oobts-test-user').addClass('oobts-common');
			}, 100);
		} // if
	}, // businessTypeSelected

	inputChanged: function(evt) {
		if (evt && (evt.type === 'change') && (evt.target.id === 'TypeOfBusiness'))
			this.typeOfBusinessChanged();

		var enabled = this.isEnabled();

		$('.btn-continue').toggleClass('disabled', !enabled);
		this.$el.find('.cashInput').moneyFormat();
	}, // inputChanged

	isEnabled: function() {
		var enabled = this.validator.checkForm();

		if (enabled && this.CompanyView)
			enabled = this.CompanyView.readyToContinue();

		return enabled;
	}, // isEnabled

	typeOfBusinessChanged: function() {
		var name = this.$el.find('#TypeOfBusiness').val().toLowerCase();

		var companyType = this.companyTypes[name];
		if (!companyType) {
			if (this.CompanyView) {
				this.$el.find('.after-financial-details').html().insertAfter(this.$el.find('.industry-type-div'));
				this.CompanyView.$el.remove();
				this.CompanyView = null;
			} // if

			this.$el.find('.WebSiteTurnOver, .OverallTurnOver').addClass('hide');

			return false;
		} // if

		this.$el.find('.WebSiteTurnOver, .OverallTurnOver').removeClass('hide');

		if (this.CompanyView && this.CompanyView.ViewName !== companyType.Type) {
			this.$el.find('.after-financial-details').insertAfter(this.$el.find('.industry-type-div'));
			this.CompanyView.$el.remove();
			this.CompanyView = null;
		} // if

		if (!this.CompanyView) {
			if (companyType.View) {
				this.CompanyView = new companyType.View({ model: this.model, parentView: this });
				this.CompanyView.on('next', this.next, this);
				this.CompanyView.render();
				this.CompanyView.$el.appendTo(this.$el.find('.company-full-details'));
				this.$el.find('.after-financial-details').insertAfter(this.$el.find('.financial-details'));
			} // if can create view

			this.inputChanged();
		}
		else
			this.CompanyView.$el.show();

		// if (!this.curOobts) this.$el.find('.after-business-type *').disable();

		return false;
	}, // typeOfBusinessChanged

	overallTurnOverFocus: function() { $('#OverallTurnOver').change(); }, // overallTurnOverFocus

	webSiteTurnOverFocus: function() { $('#WebSiteTurnOver').change(); }, // webSiteTurnOverFocus

	render: function() {
		UnBlockUi();
		this.$el.html(this.template(this.model.toJSON()));

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.validator = this.$el.find('.CompanyDetailForm').validate({
			rules: this.validatorRules,
			messages: this.validatorMessages,
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
			ignore: ':not(:visible):not(.director_birth_date)',
		});

		this.$el.find('.cashInput').moneyFormat();

		this.$el.find('#TypeOfBusiness').val('Limited').change().attardi_labels('toggle');
		this.$el.find('#TypeOfBusinessImage').field_status('set', 'ok');

		// if (!this.curOobts) this.$el.find('.after-business-type *').disable();

		this.readyToProceed = true;
		return this;
	}, // render

	ownValidationRules: function() {
		return {
			TypeOfBusiness: { required: true },
			IndustryType: { required: true },
			DirectorCheck: { required: true },
			VatReporting: { required: true },
		};
	}, // ownValidationRules

	ownValidationMessages: function() {
		return {};
	}, // ownValidationMessages

	next: function() {
		if ($('#TypeOfBusiness').val() !== "Entrepreneur")
			if (!this.$el.find('input#DirectorCheck').is(":checked"))
				EzBob.App.trigger('error', 'You must agree to director constraint in order to continue.');

		if ($('.btn-continue').hasClass('disabled'))
			return false;

		BlockUi();

		var form = this.$el.find('form.CompanyDetailForm'),
			data = form.serializeArray();

		var action = form.attr('action'),
			dataForCompany = SerializeArrayToEasyObject(data),
			typeOfBussiness = dataForCompany.TypeOfBusiness,
			companyName = null,
			postcode = null,
			sCompanyFilter = '',
			refNum = '',
			bDoTargeting = false;

		if (typeOfBussiness === 'Entrepreneur') {
			if (EzBob.Config.TargetsEnabledEntrepreneur) {
				postcode = this.model.get('PersonalAddress').models[0].get('Postcode');

				companyName = (function(model) {
					var cpi = model.get('CustomerPersonalInfo');
					return cpi.FirstName + ' ' + cpi.Surname;
				})(this.model);

				sCompanyFilter = 'N';

				bDoTargeting = true;
			} // if
		}
		else {
			switch (this.companyTypes[typeOfBussiness.toLowerCase()].Type) {
				case 'Limited':
					postcode = dataForCompany['LimitedCompanyAddress[0].Postcode'];
					companyName = dataForCompany.LimitedCompanyName;
					refNum = dataForCompany.LimitedCompanyNumber;
					sCompanyFilter = 'L';
					bDoTargeting = true;
					break;

				case 'NonLimited':
					postcode = dataForCompany['NonLimitedCompanyAddress[0].Postcode'];
					companyName = dataForCompany.NonLimitedCompanyName;
					sCompanyFilter = 'N';
					bDoTargeting = true;
					break;
			} // switch type of business
		} // if

		if (EzBob.Config.TargetsEnabled && bDoTargeting)
			this.handleTargeting(form, action, data, postcode, companyName, sCompanyFilter, refNum);
		else
			this.saveDataRequest(action, data);

		return false;
	}, // next

	handleTargeting: function(form, action, data, postcode, companyName, sCompanyFilter, refNum) {
		var that = this;

		var req = $.get(window.gRootPath + 'Account/CheckingCompany', { companyName: companyName, postcode: postcode, filter: sCompanyFilter, refNum: refNum });

		scrollTop();
		req.done(function(reqData) {
			if (!reqData)
				that.saveDataRequest(action, data);
			else {
				switch (reqData.length) {
					case 0:
						if (that.targetingTries === 0) {
							EzBob.App.trigger('warning', 'Company ' + companyName + ' ' + postcode + ' was not found. Please check your input and try again.');
							that.targetingTries++;
							UnBlockUi();
						}
						else
							that.saveTargeting(null, action, form);

						break;

					case 1:
						that.saveTargeting(reqData[0], action, form);
						break;

					default:
						var companyTargets = new EzBob.companyTargets({ model: reqData });
						companyTargets.render();
						UnBlockUi();
						companyTargets.on('BusRefNumGetted', function(targetingData) {
							BlockUi();
							that.saveTargeting(targetingData, action, form);
						});
						break;
				} // switch reqData.length
			} // if
		}); // on done

		req.error(function() {
			UnBlockUi();
		});
	}, // handleTargeting

	saveTargeting: function(targetingData, action, form) {
		var data = form.serializeArray();

		if (targetingData && targetingData.BusRefNum != 'skip') {
			data.push({ name: "AddrLine1", value: targetingData.AddrLine1 });
			data.push({ name: "AddrLine2", value: targetingData.AddrLine2 });
			data.push({ name: "AddrLine3", value: targetingData.AddrLine3 });
			data.push({ name: "AddrLine4", value: targetingData.AddrLine4 });
			data.push({ name: "PostCode", value: targetingData.PostCode });
			data.push({ name: "BusName", value: targetingData.BusName });
			data.push({ name: "BusRefNum", value: targetingData.BusRefNum });
		} else
			data.push({ name: "BusRefNum", value: "NotFound" });

		this.saveDataRequest(action, data);
	}, // saveTargeting

	saveDataRequest: function(action, data) {
		var that = this;

		if (this.$el.find('#OverallTurnOver').is(":visible"))
			_.find(data, function(d) { return d.name === 'OverallTurnOver'; }).value = this.$el.find('#OverallTurnOver').autoNumericGet();

		var pbo = _.find(data, function(d) { return d.name === 'PartBusinessOnline'; });
		if (pbo)
			pbo.value = this.curOobts === 'online';
		else
			data.push({ name: 'PartBusinessOnline', value: this.curOobts === 'online' });

		if (this.$el.find('#DirectorCheck').is(":checked"))
			_.find(data, function(d) { return d.name === 'DirectorCheck'; }).value = true;

		var totalMonthlySalary = _.find(data, function(d) { return d.name === 'TotalMonthlySalary'; });
		if (totalMonthlySalary)
			totalMonthlySalary.value = this.$el.find('#TotalMonthlySalary').autoNumericGet();

		var capitalExpenditure = _.find(data, function(d) { return d.name === 'CapitalExpenditure'; });
		if (capitalExpenditure)
			capitalExpenditure.value = this.$el.find('#CapitalExpenditure').autoNumericGet();

		var request = $.post(action, data);

		request.success(function(res) {
			scrollTop();

			if (res.error) {
				EzBob.App.trigger('error', res.error);
				UnBlockUi();
				return;
			} // if

			that.model.fetch().done(function() {
				that.trigger('ready');
				EzBob.App.trigger('clear');
				that.trigger('next');
			});
		});

		request.fail(function() {
			EzBob.App.trigger('error', 'Failed to save company details.');
			UnBlockUi();
		});
	}, // saveDataRequest
}); // EzBob.CompanyDetailsStepView
