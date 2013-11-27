var EzBob = EzBob || {};

EzBob.CompanyDetailsStepModel = EzBob.WizardStepModel.extend({});

EzBob.CompanyDetailsStepView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#company-data-template').html());

		this.companyTypes = {
			entrepreneur: {
				View: null,
				Type: 'entrepreneur',
				ClassName: null,
				Validator: EzBob.validateCompanyDetailsMetaData,
				ValidatorEx: null,
			},

			soletrader: {
				View: EzBob.NonLimitedInformationView,
				Type: 'NonLimited',
				ClassName: 'NonLimitedCompanyDetailForm',
				Validator: EzBob.validateNonLimitedCompanyDetailForm,
				ValidatorEx: EzBob.validateCompanyDetailsMetaData,
			},

			llp: {
				View: EzBob.LimitedInformationView,
				Type: 'Limited',
				ClassName: 'LimitedCompanyDetailForm',
				Validator: EzBob.validateLimitedCompanyDetailForm,
				ValidatorEx: EzBob.validateCompanyDetailsMetaData,
			},
		}; // companyTypes

		this.companyTypes.pship3p = this.companyTypes.soletrader;
		this.companyTypes.pship = this.companyTypes.soletrader;
		this.companyTypes.limited = this.companyTypes.llp;

		var companyTypeClassNames = {};
		var aryClassNames = [];

		for (var idx in this.companyTypes) {
			var ct = this.companyTypes[idx];

			if (!ct.ClassName)
				continue;

			if (!companyTypeClassNames[ct.ClassName]) {
				companyTypeClassNames[ct.ClassName] = 1;
				aryClassNames.push(ct.ClassName);
			} // if
		} // for

		this.companyTypeAllClassNames = aryClassNames.join(' ');

		this.parentView = this.options.parentView;

		this.events = _.extend({}, this.events, {
			'click .btn-continue': 'next',

			'change #TypeOfBusiness': 'typeOfBusinessChanged',
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

		this.validators = null;

		this.readyToProceed = false;

		this.constructor.__super__.initialize.call(this);
	}, // initialize

	inputChanged: function() {
		var enabled = this.validators ? true : false;

		var oForm = null;

		for (var i = 0; i < this.validators.length; i++) {
			if (!oForm)
				oForm = this.$el.find('.CompanyDetailForm');

			var oValidator = this.validators[i](oForm);

			enabled = enabled && EzBob.Validation.checkForm(oValidator);
		} // for

		if (this.CompanyView)
			enabled = enabled && this.CompanyView.readyToContinue();

		$('.continue').toggleClass('disabled', !enabled);
	}, // inputChanged

	typeOfBusinessChanged: function() {
		var name = this.$el.find('select[name="TypeOfBusiness"]').val().toLowerCase();

		var oForm = this.$el.find('.CompanyDetailForm');

		oForm.removeClass(this.companyTypeAllClassNames);

		var companyType = this.companyTypes[name];
		if (!companyType) {
			this.setValidator();
			return false;
		} // if

		if (this.CompanyView && this.CompanyView.ViewName !== companyType.Type) {
			this.CompanyView.$el.remove();
			this.CompanyView = null;
		} // if

		if (!companyType.View) {
			this.setValidator();
			return false;
		} // if

		if (!this.CompanyView) {
			this.CompanyView = new companyType.View({ model: this.model, parentView: this });
			this.CompanyView.on('next', this.next, this);
			this.CompanyView.$el.appendTo(this.$el.find('.company-full-details'));
			this.CompanyView.render();

			oForm.addClass(companyType.ClassName);
			this.setValidator();
		}
		else
			this.CompanyView.$el.show();

		return false;
	}, // typeOfBusinessChanged

	overallTurnOverFocus: function() { $('#OverallTurnOver').change(); }, // overallTurnOverFocus

	webSiteTurnOverFocus: function() { $('#WebSiteTurnOver').change(); }, // webSiteTurnOverFocus

	render: function() {
		this.$el.html(this.template(this.model.toJSON()));

		this.$el.find('.cashInput').moneyFormat();

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.typeOfBusinessChanged();
		this.inputChanged();

		this.readyToProceed = true;
		return this;
	}, // render

	setValidator: function() {
		var name = this.$el.find('select[name="TypeOfBusiness"]').val().toLowerCase();
		var companyType = this.companyTypes[name];

		if (!companyType) {
			this.validators = [EzBob.validateCompanyDetailsMetaData];
			return;
		} // if

		this.validators = [companyType.Validator];
		if (companyType.ValidatorEx)
			this.validators.push(companyType.ValidatorEx);
	}, // setValidator

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

		if (this.isInCompanyMode && typeOfBussiness !== 'Entrepreneur' && EzBob.Config.TargetsEnabled)
			this.handleTargeting(form, action, data, postcode, companyName, sCompanyFilter, refNum);
		else
			this.saveDataRequest(action, data);

		return false;
	},

	setRefNum: function(refNum) {
		$('.RefNum').val(refNum);
	},

	handleTargeting: function(form, action, data, postcode, companyName, sCompanyFilter, refNum) {
		var that = this;

		var req = $.get(window.gRootPath + 'Account/CheckingCompany', { companyName: companyName, postcode: postcode, filter: sCompanyFilter, refNum: refNum });

		scrollTop();
		BlockUi();
		req.success(function(reqData) {
			if (!reqData || !reqData.success)
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
		});

		req.complete(function() {
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
	} // saveDataRequest
}); // EzBob.CompanyDetailsStepView
