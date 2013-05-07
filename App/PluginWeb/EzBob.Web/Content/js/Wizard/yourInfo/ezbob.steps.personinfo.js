/// <reference path="~/Content/js/App/ezbob.clicktale.js" />
/// <reference path="../../ezbob.design.js" />
/// <reference path="~/Content/js/App/ezbob.app.js" />
/// <reference path="~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js" />


var EzBob = EzBob || {};

EzBob.YourInformationStepModel = EzBob.WizardStepModel.extend({
});

EzBob.YourInformationStepView = Backbone.View.extend({
    initialize: function () {
        this.types = {
            entrepreneur: EzBob.PersonalInformationView,
            pship3p: EzBob.NonLimitedInformationView,
            pship: EzBob.NonLimitedInformationView,
            llp: EzBob.LimitedInformationView,
            limited: EzBob.LimitedInformationView,
            soletrader: EzBob.NonLimitedInformationView
        };
        this.thankYouPage = new EzBob.ThankYouWizardPage();
        this.thankYouPage.$el.appendTo(this.$el);

        this.on('ready', function () {
            EzBob.CT.recordEvent('ct:personalinfo.complete');
        });

        EzBob.App.on('ct:personalinfo.show', this.showStep, this);
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
    showStep: function (name) {
        this.PersonalView.$el.hide();
        if (name === "entrepreneur") {
            EzBob.App.trigger("wizard:progress", 70);
            this.infoView.$el.hide();
            this.PersonalView.$el.show();
            return;
        }

        var infoType = this.types[name.toLowerCase()];
        if (!infoType) return;

        if (this.infoView) {
            this.infoView.$el.empty();
        }

        this.infoView = new infoType({ model: this.model });
        this.infoView.$el.appendTo(this.$el);
        this.infoView.render();
    },
    ct_complete: function () {
        this.infoView.$el.hide();
        this.PersonalView.$el.hide();
        this.thankYouPage.render();
    },
    next: function nextStep(name) {       
        if (name.toLowerCase() == 'entrepreneur') {
            this.saveData();
            return false;
        }
        var infoType = this.types[name.toLowerCase()];
        if (!infoType) return false;

        EzBob.CT.recordEvent('ct:personalinfo.show', name);
        EzBob.App.trigger("wizard:progress", 90);

        this.PersonalView.$el.hide();
        
        this.infoView = new infoType({ model: this.model });
        this.infoView.$el.appendTo(this.$el);
        this.infoView.render();

        this.infoView.on('back', this.backToPersonal, this);
        this.infoView.on('next', this.saveData, this);
        return false;
    },
    back: function () {
        this.trigger('previous');
    },
    backToPersonal: function () {
        EzBob.App.trigger("wizard:progress", 70);
        scrollTop();
        EzBob.CT.recordEvent('ct:personalinfo.show', 'personal');
        this.infoView.$el.hide();
        this.PersonalView.$el.show();
    },
    complete: function () {
        this.saveData();
    },
    saveData: function () {
        var form = this.$el.find('form.CompanyDetailForm'),
            data = form.serializeArray();
        
        _.find(data, function (d) { return d.name == "OverallTurnOver"; }).value = this.$el.find("#OverallTurnOver").autoNumericGet();
        _.find(data, function (d) { return d.name == "WebSiteTurnOver"; }).value = this.$el.find("#WebSiteTurnOver").autoNumericGet();
        
        var action = form.attr('action'),
            dataForCompany = SerializeArrayToEasyObject(data),
            typeOfBussiness = dataForCompany.TypeOfBusiness,
            companyName = null,
            postcode = null,
            that = this;
        
        EzBob.App.trigger("wizard:progress", 100);
        
        switch (typeOfBussiness.toLowerCase()) {
            case "entrepreneur":
                break;

            case "llp":
            case "limited":
                postcode = dataForCompany['LimitedCompanyAddress[0].Postcode'];
                companyName = dataForCompany['LimitedCompanyName'];
                break;

            case "pship":
            case "pship3p":
            case "soletrader":
                postcode = dataForCompany['NonLimitedCompanyAddress[0].Postcode'];
                companyName = dataForCompany['NonLimitedCompanyName'];
                break;

            default:
        }

        if (typeOfBussiness != "Entrepreneur" && EzBob.Config.TargetsEnabled) {
            var req = $.get(window.gRootPath + "Account/CheckingCompany", { companyName: companyName, postcode: postcode });
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
        var request = $.post(action, data);
        request.success(function (res) {
            if (res.error) {
                EzBob.App.trigger('error', res.error);
                return;
            }
            
            that.PersonalView.$el.hide();
            if (that.infoView) that.infoView.$el.hide();
            
            that.thankYouPage.render();
            that.trigger('ready');
            scrollTop();
            
        });

        request.complete(function() {
            BlockUi("Off");
        });
    }
});
//--------------------------------------------------------------------------------------
EzBob.ThankYouWizardPage = Backbone.View.extend({
    initialize: function () {
        this.template = $('#lastWizardThankYouPage').html();
    },
    render: function () {
        this.$el.html(this.template);
        $('.sidebarBox').find('li[rel]').setPopover('left');
        return this;
    }
});