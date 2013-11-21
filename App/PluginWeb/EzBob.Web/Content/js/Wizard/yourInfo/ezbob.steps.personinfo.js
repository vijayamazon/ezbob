/// <reference path="~/Content/js/App/ezbob.clicktale.js" />
/// <reference path="../../ezbob.design.js" />
/// <reference path="~/Content/js/App/ezbob.app.js" />
/// <reference path="~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js" />

var EzBob = EzBob || {};

EzBob.YourInformationStepModel = EzBob.WizardStepModel.extend({});

EzBob.YourInformationStepView = Backbone.View.extend({
    initialize: function () {
        this.companyTypes = {
            entrepreneur: { View: EzBob.PersonalInformationView, Type: "entrepreneur" },
            pship3p: { View: EzBob.NonLimitedInformationView, Type: "NonLimited" },
            pship: { View: EzBob.NonLimitedInformationView, Type: "NonLimited" },
            llp: { View: EzBob.LimitedInformationView, Type: "Limited" },
            limited: { View: EzBob.LimitedInformationView, Type: "Limited" },
            soletrader: { View: EzBob.NonLimitedInformationView, Type: "NonLimited" },
        };
        
        this.PersonalView = new EzBob.PersonalInformationView({ model: this.model });
        this.PersonalView.on('next', this.next, this);
        
        this.on('ready', function () {
            EzBob.CT.recordEvent('ct:personalinfo.complete');
        });

        EzBob.App.on('ct:personalinfo.complete', this.ct_complete, this);

        this.isInCompanyMode = false;
    },

    render: function () {
        this.PersonalView.render();
        
        this.PersonalView.$el.appendTo(this.$el);
        this.PersonalView.$el.find('.addressCaption').hide();
        return this;
    },

    ct_complete: function () {
        this.infoView.$el.hide();
        this.PersonalView.$el.hide();
    },

    next: function (e) {
        var form = this.$el.find('form.CompanyDetailForm'),
            data = form.serializeArray();

        var action = form.attr('action'),
            dataForCompany = SerializeArrayToEasyObject(data),
            typeOfBussiness = dataForCompany.TypeOfBusiness,
            companyName = null,
            postcode = null,
            sCompanyFilter = '',
            refNum = "";

        if (this.isInCompanyMode) {
            switch (this.companyTypes[typeOfBussiness.toLowerCase()].Type) {
            default:
            case "entrepreneur":
                break;
            case "Limited":
                postcode = dataForCompany['LimitedCompanyAddress[0].Postcode'];
                companyName = dataForCompany['LimitedCompanyName'];
                refNum = dataForCompany['LimitedCompanyNumber'];
                sCompanyFilter = 'L';
                break;
            case "NonLimited":
                postcode = dataForCompany['NonLimitedCompanyAddress[0].Postcode'];
                companyName = dataForCompany['NonLimitedCompanyName'];
                sCompanyFilter = 'N';
                break;
            } // switch type of business
        }
        
        if (this.isInCompanyMode && typeOfBussiness != "Entrepreneur" && EzBob.Config.TargetsEnabled) {
            this.handleTargeting(form, action, data, postcode, companyName, sCompanyFilter, refNum);
        } else {
            this.saveDataRequest(action, data);
        }

        return false;
    },

    handleTargeting: function (form, action, data, postcode, companyName, sCompanyFilter, refNum) {
        var that = this;
        var req = $.get(window.gRootPath + "Account/CheckingCompany", { companyName: companyName, postcode: postcode, filter: sCompanyFilter, refNum: refNum });
        scrollTop();
        BlockUi("On");
        req.success(function (reqData) {
            if (reqData == undefined || reqData.success == false)
                that.saveDataRequest(action, data);
            else {
                switch (reqData.length) {
                    case 0:
                        EzBob.App.trigger('warning', "Company was not found by post code. Please check your input and try again.");
                        break;
                    case 1:
                        that.setRefNum(reqData[0].BusRefNum);
                        data = form.serializeArray();
                        that.saveDataRequest(action, data);
                        break;
                    default:
                        var companyTargets = new EzBob.companyTargets({ model: reqData });
                        companyTargets.render();

                        companyTargets.on("BusRefNumGetted", function (busRefNum) {
                            that.setRefNum(busRefNum);
                            data = form.serializeArray();
                            that.saveDataRequest(action, data);
                        });
                        break;
                } // switch reqData.length
            } // if
        });

        req.complete(function () {
            BlockUi("Off");
        });
    },

    jumpToCompanyMode: function () {
        var name = this.$el.find('select[name="TypeOfBusiness"]').val();
        if (name.toLowerCase() == 'entrepreneur') {
            this.trigger('ready');
            this.trigger('next');
            return false;
        } // if

        this.isInCompanyMode = true;

        EzBob.App.trigger('instep-progress-changed', 1);

        var companyType = this.companyTypes[name.toLowerCase()];
        if (!companyType)
            return false;

        this.PersonalView.$el.hide();
        if (this.CompanyView && this.CompanyView.ViewName !== companyType.Type) {
            this.CompanyView.$el.empty();
            this.CompanyView = null;
        }

        if (!this.CompanyView) {
            this.CompanyView = new companyType.View({ model: this.model });
            this.CompanyView.on('back', this.back, this);
            this.CompanyView.on('next', this.next, this);
            this.CompanyView.$el.appendTo(this.$el);
            this.CompanyView.render();

            
        } else {
            this.CompanyView.$el.show();
        }
        return false;
    },

    back: function () {
        this.isInCompanyMode = false;

        scrollTop();

        this.CompanyView.$el.hide();
        this.PersonalView.$el.show();
        this.PersonalView.inputChanged();

        EzBob.App.trigger('instep-progress-changed', 0);
    },

    setRefNum: function (refNum) {
        $(".RefNum").val(refNum);
    },

    saveDataRequest: function (action, data) {
        BlockUi("on");

        var that = this;

        _.find(data, function (d) { return d.name == "OverallTurnOver"; }).value = this.$el.find("#OverallTurnOver").autoNumericGet();
        _.find(data, function (d) { return d.name == "WebSiteTurnOver"; }).value = this.$el.find("#WebSiteTurnOver").autoNumericGet();

        data.push({ name: "isInCompanyMode", value: this.isInCompanyMode });

        var totalMonthlySalary = _.find(data, function (d) { return d.name == "TotalMonthlySalary"; });
        if (totalMonthlySalary)
            totalMonthlySalary.value = this.$el.find("#TotalMonthlySalary").autoNumericGet();

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
            BlockUi("Off");
        });
    } // saveDataRequest
});
