var EzBob = EzBob || {};

EzBob.CompanyDetailsStepModel = EzBob.WizardStepModel.extend({});

EzBob.CompanyDetailsStepView = Backbone.View.extend({
	initialize: function() {
		this.template = _.template($('#company-data-template').html());

		this.companyTypes = {
			entrepreneur: { View: null, Type: 'entrepreneur', ClassName: null },
			pship3p: { View: EzBob.NonLimitedInformationView, Type: 'NonLimited', ClassName: 'NonLimitedCompanyDetailForm' },
			pship: { View: EzBob.NonLimitedInformationView, Type: 'NonLimited', ClassName: 'NonLimitedCompanyDetailForm' },
			llp: { View: EzBob.LimitedInformationView, Type: 'Limited', ClassName: 'LimitedCompanyDetailForm' },
			limited: { View: EzBob.LimitedInformationView, Type: 'Limited', ClassName: 'LimitedCompanyDetailForm' },
			soletrader: { View: EzBob.NonLimitedInformationView, Type: 'NonLimited', ClassName: 'NonLimitedCompanyDetailForm' },
		};

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
			'focus #OverallTurnOver': 'overallTurnOverFocus',
			'focus #WebSiteTurnOver': 'webSiteTurnOverFocus',
			'change #TypeOfBusiness': 'typeOfBusinessChanged',
		}); // events

		this.readyToProceed = false;

		this.constructor.__super__.initialize.call(this);
	}, // initialize

	typeOfBusinessChanged: function() {
		var name = this.$el.find('select[name="TypeOfBusiness"]').val().toLowerCase();

		var oForm = this.CompanyDetailsView.$el.find('.CompanyDetailForm');

		oForm.removeClass(this.companyTypeAllClassNames);

		var companyType = this.companyTypes[name];
		if (!companyType)
			return false;

		if (this.CompanyView && this.CompanyView.ViewName !== companyType.Type) {
			this.CompanyView.$el.empty();
			this.CompanyView = null;
		} // if

		if (!companyType.View)
			return false;

		if (!this.CompanyView) {
			this.CompanyView = new companyType.View({ model: this.model });
			this.CompanyView.on('next', this.next, this);
			this.CompanyView.$el.appendTo(this.CompanyDetailsView.$el.find('.company-full-details'));
			this.CompanyView.render();

			oForm.addClass(companyType.ClassName);
		} else
			this.CompanyView.$el.show();

		return false;
	}, // typeOfBusinessChanged

	overallTurnOverFocus: function() { $('#OverallTurnOver').change(); }, // overallTurnOverFocus

	webSiteTurnOverFocus: function() { $('#WebSiteTurnOver').change(); }, // webSiteTurnOverFocus

	render: function() {
		this.$el.html(this.template(this.model.toJSON()));
		this.readyToProceed = false;
		return this;
	}, // render

	next: function(e) {
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

    saveDataRequest: function (action, data) {
        BlockUi();

        var that = this;

	    if (this.isInCompanyMode) {
			_.find(data, function(d) { return d.name === 'OverallTurnOver'; }).value = this.$el.find('#OverallTurnOver').autoNumericGet();
			_.find(data, function(d) { return d.name === 'WebSiteTurnOver'; }).value = this.$el.find('#WebSiteTurnOver').autoNumericGet();
		} // if

	    data.push({ name: 'isInCompanyMode', value: this.isInCompanyMode });

        var totalMonthlySalary = _.find(data, function (d) { return d.name === 'TotalMonthlySalary'; });
        if (totalMonthlySalary)
            totalMonthlySalary.value = this.$el.find('#TotalMonthlySalary').autoNumericGet();

        var request = $.post(action, data);

        request.success(function (res) {
            scrollTop();

            if (res.error) {
                EzBob.App.trigger('error', res.error);
                return;
            } // if
            
            that.model.fetch().done(function () {
                that.PersonalView.render();
                that.PersonalView.$el.hide();

                if (that.CompanyView) {
                    that.CompanyView.$el.hide();
                }

                if (that.isInCompanyMode) {
                    that.trigger('ready');
                    that.trigger('next');
                } else {
                    that.jumpToCompanyMode();
                }
            });
        });

        request.complete(function () {
            UnBlockUi();
        });
    } // saveDataRequest
}); // EzBob.CompanyDetailsStepView
