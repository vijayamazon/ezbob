﻿var EzBob = EzBob || {};

EzBob.CompanyDetailsStepModel = EzBob.WizardStepModel.extend({});

EzBob.CompanyDetailsStepView = Backbone.View.extend({
    initialize: function () {
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
            'click .btn-continue': 'next',
            'click label[for="ConsentToSearch"] a': 'showConsent',
            
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

    showConsent: function () {
        var consentAgreementModel = new EzBob.ConsentAgreementModel({
            id: this.model.get('Id'),
            firstName: this.$el.find('input[name="FirstName"]').val(),
            middleInitial: this.$el.find('input[name="MiddleInitial"]').val(),
            surname: this.$el.find('input[name="Surname"]').val()
        });

        var consentAgreement = new EzBob.ConsentAgreement({ model: consentAgreementModel });
        EzBob.App.modal.show(consentAgreement);
        return false;
    }, // showConsent
    
    inputChanged: function (evt) {
        if (evt && (evt.type === 'change') && (evt.target.id === 'TypeOfBusiness'))
            this.typeOfBusinessChanged();

        var enabled = this.validator.checkForm();

        if (enabled && this.CompanyView)
            enabled = this.CompanyView.readyToContinue();

        $('.btn-continue').toggleClass('disabled', !enabled);
        this.$el.find('.cashInput').moneyFormat();
    }, // inputChanged

    typeOfBusinessChanged: function () {
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

    overallTurnOverFocus: function () { $('#OverallTurnOver').change(); }, // overallTurnOverFocus

    webSiteTurnOverFocus: function () { $('#WebSiteTurnOver').change(); }, // webSiteTurnOverFocus

    render: function () {
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

        this.readyToProceed = true;
        return this;
    }, // render

    ownValidationRules: function () {
        var overallRegex = "^(?!£ 0.00$)";

        return {
            TypeOfBusiness: { required: true },
            IndustryType: { required: true },
            ConsentToSearch: { required: true },
            DirectorCheck: { required: true },
            OverallTurnOver: { required: true, defaultInvalidPounds: true, regex: overallRegex },
        };
    }, // ownValidationRules

    ownValidationMessages: function () {
        return {
            OverallTurnOver: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
        };
    }, // ownValidationMessages

    next: function (e) {
        if ($('#TypeOfBusiness').val() == "Entrepreneur") {
            if (!$('input[name="ConsentToSearch"]').prop('Checked')) {
                EzBob.App.trigger('error', 'You must agree to terms and conditions in order to continue.');
            }
        } else {
            if (!$('input[name="DirectorCheck"]').prop('Checked') || !$('input[name="ConsentToSearch"]').prop('Checked')) {
                EzBob.App.trigger('error', 'You must agree to director and consent in order to continue.');
            }
        }
        
        if ($('.btn-continue').hasClass('disabled'))
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
            this.handleTargeting(form, action, data, postcode, companyName, sCompanyFilter, refNum, typeOfBussiness);
        else
            this.saveDataRequest(action, data, typeOfBussiness);

        return false;
    },

    handleTargeting: function (form, action, data, postcode, companyName, sCompanyFilter, refNum, typeOfBussiness) {
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
                        if (that.targetingTries == 0) {
                            EzBob.App.trigger('warning', 'Company ' + companyName + ' ' + postcode + ' was not found. Please check your input and try again.');
                            that.targetingTries++;
                        } else {
                            that.saveTargeting(null, action, form, typeOfBussiness);
                        }
                        break;
                    case 1:
                        that.saveTargeting(reqData[0], action, form, typeOfBussiness);
                        break;
                    default:
                        var companyTargets = new EzBob.companyTargets({ model: reqData });
                        companyTargets.render();
                        companyTargets.on('BusRefNumGetted', function (targetingData) {
                            that.saveTargeting(targetingData, action, form, typeOfBussiness);
                        });
                        break;
                } // switch reqData.length
            } // if
        }); // on done

        req.always(function () {
            UnBlockUi();
        });
    }, // handleTargeting
    saveTargeting: function (targetingData, action, form, typeOfBussiness) {
        var that = this;
        var data = form.serializeArray();
        if (targetingData) {
            data.push({ name: "AddrLine1", value: targetingData.AddrLine1 });
            data.push({ name: "AddrLine2", value: targetingData.AddrLine2 });
            data.push({ name: "AddrLine3", value: targetingData.AddrLine3 });
            data.push({ name: "AddrLine4", value: targetingData.AddrLine4 });
            data.push({ name: "PostCode", value: targetingData.PostCode });
            data.push({ name: "BusName", value: targetingData.BusName });
            data.push({ name: "BusRefNum", value: targetingData.BusRefNum });
        } else {
            data.push({ name: "BusRefNum", value: "NotFound" });
        }
        this.saveDataRequest(action, data);
    },
    saveDataRequest: function (action, data, typeOfBussiness) {
        BlockUi();

        var that = this;
        if (this.$el.find('#OverallTurnOver').is(":visible")) {
            _.find(data, function (d) { return d.name === 'OverallTurnOver'; }).value = this.$el.find('#OverallTurnOver').autoNumericGet();
        }
        
        if (this.$el.find('#ConsentToSearch').is(":checked")) {
            _.find(data, function (d) { return d.name === 'ConsentToSearch'; }).value = true;
        }
        if (this.$el.find('#PartBusinessOnline').is(":checked")) {
            _.find(data, function (d) { return d.name === 'PartBusinessOnline'; }).value = true;
        }
        if (this.$el.find('#DirectorCheck').is(":checked")) {
            _.find(data, function (d) { return d.name === 'DirectorCheck'; }).value = true;
        }

        var totalMonthlySalary = _.find(data, function (d) { return d.name === 'TotalMonthlySalary'; });
        if (totalMonthlySalary)
            totalMonthlySalary.value = this.$el.find('#TotalMonthlySalary').autoNumericGet();

        var capitalExpenditure = _.find(data, function (d) { return d.name === 'CapitalExpenditure'; });
        if (capitalExpenditure)
            capitalExpenditure.value = this.$el.find('#CapitalExpenditure').autoNumericGet();

        console.log('data', data);
        debugger;
        var request = $.post(action, data);

        request.success(function (res) {
            scrollTop();

            if (res.error) {
                EzBob.App.trigger('error', res.error);
                return;
            } // if

            that.model.fetch().done(function () {
                that.trigger('ready');
                EzBob.App.trigger('clear');
                that.trigger('next');
            });
        });
        request.complete(function () {
            if (typeOfBussiness !== 'Entrepreneur') {
                $.post("" + window.gRootPath + "Customer/Experian/PerformCompanyCheck", {});
                $.post("" + window.gRootPath + "Customer/Experian/PerformConsumerCheckForDirectors", {});
            }

            UnBlockUi();
        });
    }, // saveDataRequest
}); // EzBob.CompanyDetailsStepView
