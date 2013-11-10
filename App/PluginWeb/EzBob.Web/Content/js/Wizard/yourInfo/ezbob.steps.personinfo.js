/// <reference path="~/Content/js/App/ezbob.clicktale.js" />
/// <reference path="../../ezbob.design.js" />
/// <reference path="~/Content/js/App/ezbob.app.js" />
/// <reference path="~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js" />


var EzBob = EzBob || {};

EzBob.YourInformationStepModel = EzBob.WizardStepModel.extend({
});

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
        this.thankYouPage = new EzBob.ThankYouWizardPage({ model: this.model });
        this.thankYouPage.$el.appendTo(this.$el);

        this.on('ready', function () {
            EzBob.CT.recordEvent('ct:personalinfo.complete');
        });

        EzBob.App.on('ct:personalinfo.complete', this.ct_complete, this);
    },
    render: function () {
        this.PersonalView = new EzBob.PersonalInformationView({ model: this.model });
        this.PersonalView.render();
        this.PersonalView.on('next', this.next, this);
        this.PersonalView.on('back', this.back, this);
        this.PersonalView.$el.appendTo(this.$el);
        this.PersonalView.$el.find('.addressCaption').hide();
        return this;
    },
    ct_complete: function () {
        this.CompanyView.$el.hide();
        this.PersonalView.$el.hide();
        this.thankYouPage.render();
    },
    next: function nextStep(name) {
        if (name.toLowerCase() == 'entrepreneur') {
            this.saveData();
            return false;
        }
        var companyType = this.companyTypes[name.toLowerCase()];
        if (!companyType) return false;
        EzBob.CT.recordEvent('ct:personalinfo.show', name);
        EzBob.App.trigger("wizard:progress", 90);

        this.PersonalView.$el.hide();

        if (this.CompanyView && this.CompanyView.ViewName !== companyType.Type) {
            this.CompanyView.$el.empty();
            this.CompanyView = null;
        }

        if (!this.CompanyView) {
            this.CompanyView = new companyType.View({ model: this.model });

            this.CompanyView.$el.appendTo(this.$el);
            this.CompanyView.render();

            this.CompanyView.on('back', this.backToPersonal, this);
            this.CompanyView.on('next', this.saveData, this);
        } else {
            this.CompanyView.$el.show();
        }
        return false;
    },
    backToPersonal: function () {
        EzBob.App.trigger("wizard:progress", 70);
        scrollTop();
        EzBob.CT.recordEvent('ct:personalinfo.show', 'personal');
        this.CompanyView.$el.hide();
        this.PersonalView.$el.show();
        this.PersonalView.inputChanged();
    },
    complete: function () {
        this.saveData();
    },
    saveData: function () {
        var form = this.$el.find('form.CompanyDetailForm'),
            data = form.serializeArray();

        var action = form.attr('action'),
            dataForCompany = SerializeArrayToEasyObject(data),
            typeOfBussiness = dataForCompany.TypeOfBusiness,
            companyName = null,
            postcode = null,
            that = this,
            sCompanyFilter = '',
            refNum = "";
        EzBob.App.trigger("wizard:progress", 100);

        switch (typeOfBussiness.toLowerCase()) {
            case "entrepreneur":
                break;

            case "llp":
            case "limited":
                postcode = dataForCompany['LimitedCompanyAddress[0].Postcode'];
                companyName = dataForCompany['LimitedCompanyName'];
                refNum = dataForCompany['LimitedCompanyNumber'];
                sCompanyFilter = 'L';
                break;

            case "pship":
            case "pship3p":
            case "soletrader":
                postcode = dataForCompany['NonLimitedCompanyAddress[0].Postcode'];
                companyName = dataForCompany['NonLimitedCompanyName'];
                sCompanyFilter = 'N';
                break;

            default:
        }

        if (typeOfBussiness != "Entrepreneur" && EzBob.Config.TargetsEnabled) {
            var req = $.get(window.gRootPath + "Account/CheckingCompany", { companyName: companyName, postcode: postcode, filter: sCompanyFilter, refNum: refNum });
            scrollTop();
            BlockUi("On");

            req.success(function (reqData) {
                if (reqData == undefined || reqData.success == false) {
                    that.saveDataRequest(action, data);
                } else {
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
                    }
                }
            });

            req.complete(function () {
                BlockUi("Off");
            });

        } else {
            this.saveDataRequest(action, data);
        }

        return;
    },
    setRefNum: function (refNum) {
        $(".RefNum").val(refNum);
    },
    saveDataRequest: function (action, data) {
        BlockUi("on");
        var that = this;
        _.find(data, function (d) { return d.name == "OverallTurnOver"; }).value = this.$el.find("#OverallTurnOver").autoNumericGet();
        _.find(data, function (d) { return d.name == "WebSiteTurnOver"; }).value = this.$el.find("#WebSiteTurnOver").autoNumericGet();

        var totalMonthlySalary = _.find(data, function (d) { return d.name == "TotalMonthlySalary"; });
        if (totalMonthlySalary) {
            totalMonthlySalary.value = this.$el.find("#TotalMonthlySalary").autoNumericGet();
        }
        var request = $.post(action, data);
        request.success(function (res) {
            if (res.error) {
                EzBob.App.trigger('error', res.error);
                return;
            }

            that.PersonalView.$el.hide();
            if (that.CompanyView) that.CompanyView.$el.hide();

            that.thankYouPage.render();
            that.trigger('ready');
            scrollTop();

        });

        request.complete(function () {
            BlockUi("Off");
        });
    }
});
//--------------------------------------------------------------------------------------
EzBob.ThankYouWizardPage = Backbone.View.extend({
    initialize: function () {
        this.template = $(_.template($("#lastWizardThankYouPage").html(), { ordty: ordty }));
    },
    render: function () {
        $.getJSON(window.gRootPath + "Customer/Wizard/EarnedPointsStr").done(function (data) {
            if (data.EarnedPointsStr)
                $('#EarnedPoints').text(data.EarnedPointsStr);
        });

        this.$el.html(this.template);
        $('.sidebarBox').find('li[rel]').setPopover('left');

        if (!this.model.get('IsOffline'))
            this.$el.find('.offline').remove();
        else
            this.$el.find('.notoffline').remove();

        return this;
    }
});