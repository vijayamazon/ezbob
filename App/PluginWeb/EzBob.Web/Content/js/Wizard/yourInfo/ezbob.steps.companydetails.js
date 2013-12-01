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
			'click .btn-continue': 'next',

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
	}, // initialize

	inputChanged: function(evt) {
		if (evt && (evt.type === 'change') && (evt.target.id === 'TypeOfBusiness'))
			this.typeOfBusinessChanged();

		var enabled = this.validator.checkForm();

		if (enabled && this.CompanyView)
			enabled = this.CompanyView.readyToContinue();

		$('.continue').toggleClass('disabled', !enabled);
	}, // inputChanged

	typeOfBusinessChanged: function() {
		var name = this.$el.find('#TypeOfBusiness').val().toLowerCase();

		var companyType = this.companyTypes[name];
		if (!companyType) {
			if (this.CompanyView) {
				this.CompanyView.$el.remove();
				this.CompanyView = null;
			} // if

			this.$el.find('.WebSiteTurnOver, .OverallTurnOver').addClass('hide');

			return false;
		} // if

		this.$el.find('.WebSiteTurnOver, .OverallTurnOver').removeClass('hide');

		if (this.CompanyView && this.CompanyView.ViewName !== companyType.Type) {
			this.CompanyView.$el.remove();
			this.CompanyView = null;
		} // if

		if (!this.CompanyView) {
			if (companyType.View) {
				this.CompanyView = new companyType.View({ model: this.model, parentView: this });
				this.CompanyView.on('next', this.next, this);
				this.CompanyView.render();
				this.CompanyView.$el.appendTo(this.$el.find('.company-full-details'));
			} // if can create view

			this.inputChanged();
		}
		else
			this.CompanyView.$el.show();

		return false;
	}, // typeOfBusinessChanged

	overallTurnOverFocus: function() { $('#OverallTurnOver').change(); }, // overallTurnOverFocus

	webSiteTurnOverFocus: function() { $('#WebSiteTurnOver').change(); }, // webSiteTurnOverFocus

	render: function() {
		this.$el.html(this.template(this.model.toJSON()));

		if (!this.model.get('IsOffline'))
			this.$el.find('.offline').remove();
		else
			this.$el.find('.notoffline').remove();

		this.validator = this.$el.find('.CompanyDetailForm').validate({
			rules: this.validatorRules,
			messages: this.validatorMessages,
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
			ignore: ':not(:visible):not(.director_birth_date)',
		});

		this.$el.find('.cashInput').moneyFormat();

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.inputChanged();

		this.readyToProceed = true;
		return this;
	}, // render

	ownValidationRules: function() {
		var overallRegex = "^(?!£ 0.00$)";
		var turnoverRegex = this.model.get('IsOffline') ? "^(£ 0.00$)|" + overallRegex : overallRegex;

		return {
			TypeOfBusiness: { required: true },
			OverallTurnOver: { required: true, defaultInvalidPounds: true, regex: overallRegex },
			WebSiteTurnOver: { required: true, defaultInvalidPounds: true, regex: turnoverRegex },
		};
	}, // ownValidationRules

	ownValidationMessages: function() {
		return {
			TimeAtAddress: { regex: "This field is required" },
			OverallTurnOver: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
			WebSiteTurnOver: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
		};
	}, // ownValidationMessages

	next: function(e) {
		if ($('.continue').hasClass('disabled'))
			return false;

		var form = this.$el.find('form.CompanyDetailForm'),
			data = form.serializeArray();

		var action = form.attr('action'),
			dataForCompany = SerializeArrayToEasyObject(data),
			typeOfBussiness = dataForCompany.TypeOfBusiness,
			companyName = null,
			postcode = null,
			sCompanyFilter = '',
			refNum = '';

		switch (this.companyTypes[typeOfBussiness.toLowerCase()].Type) {
			case 'Limited':
				postcode = dataForCompany['LimitedCompanyAddress[0].Postcode'];
				companyName = dataForCompany.LimitedCompanyName;
				refNum = dataForCompany.LimitedCompanyNumber;
				sCompanyFilter = 'L';
				break;

			case 'NonLimited':
				postcode = dataForCompany['NonLimitedCompanyAddress[0].Postcode'];
				companyName = dataForCompany.NonLimitedCompanyName;
				sCompanyFilter = 'N';
				break;
		} // switch type of business

		if (typeOfBussiness !== 'Entrepreneur' && EzBob.Config.TargetsEnabled)
			this.handleTargeting(form, action, data, postcode, companyName, sCompanyFilter, refNum);
		else
			this.saveDataRequest(action, data);

		return false;
	},

	setRefNum: function (refNum) {
	    $('.RefNum').val(refNum);
	}, // setRefNum

	handleTargeting: function(form, action, data, postcode, companyName, sCompanyFilter, refNum) {
		var that = this;

		var req = $.get(window.gRootPath + 'Account/CheckingCompany', { companyName: companyName, postcode: postcode, filter: sCompanyFilter, refNum: refNum });

		scrollTop();

		BlockUi();

		req.done(function (reqData) {
			if (!reqData)
				that.saveDataRequest(action, data);
			else {
				switch (reqData.length) {
				case 0:
					EzBob.App.trigger('warning', 'Company was not found by post code. Please check your input and try again.');
					break;
				case 1:
					that.setRefNum(reqData[0].BusRefNum);
					data = form.serializeArray();
					that.saveDataRequest(action, data);
					break;
				default:
					var companyTargets = new EzBob.companyTargets({ model: reqData });
					companyTargets.render();
					companyTargets.on('BusRefNumGetted', function(busRefNum) {
						that.setRefNum(busRefNum);
						data = form.serializeArray();
						that.saveDataRequest(action, data);
					});
					break;
				} // switch reqData.length
			} // if
		}); // on done

		req.always(function() {
			UnBlockUi();
		});
	}, // handleTargeting

	saveDataRequest: function(action, data) {
		BlockUi();

		var that = this;

		_.find(data, function(d) { return d.name === 'OverallTurnOver'; }).value = this.$el.find('#OverallTurnOver').autoNumericGet();
		_.find(data, function(d) { return d.name === 'WebSiteTurnOver'; }).value = this.$el.find('#WebSiteTurnOver').autoNumericGet();

		var totalMonthlySalary = _.find(data, function(d) { return d.name === 'TotalMonthlySalary'; });
		if (totalMonthlySalary)
			totalMonthlySalary.value = this.$el.find('#TotalMonthlySalary').autoNumericGet();

		var request = $.post(action, data);

		request.success(function(res) {
			scrollTop();

			if (res.error) {
				EzBob.App.trigger('error', res.error);
				return;
			} // if

			that.model.fetch().done(function() {
				that.trigger('ready');
				that.trigger('next');
			});
		});

		request.complete(function() {
			UnBlockUi();
		});
	}, // saveDataRequest
}); // EzBob.CompanyDetailsStepView
