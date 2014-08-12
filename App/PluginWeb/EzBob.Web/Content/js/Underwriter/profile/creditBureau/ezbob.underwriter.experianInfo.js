var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ExperianInfoModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/CreditBureau/Index",
    async: false
});

EzBob.Underwriter.ExperianInfoView = Backbone.View.extend({
    initialize: function() {
        this.template = _.template($('#experian-info-template').html());
        this.model.on('change sync', this.render, this);
    },
    render: function() {
	    var uiData = this.model.toJSON();

	    uiData.Consumer = uiData.Consumer || {};
	    uiData.Consumer.ConsumerAccountsOverview = uiData.Consumer.ConsumerAccountsOverview || {};

	    this.$el.html(this.template({ experianInfo: uiData }));
        this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });
        this.$el.find('.check-history tr:not(:eq(0),:eq(1))').css("cursor", "pointer");
    },
    events: {
        "click #RunConsumerCheckBtn" : "RunConsumerCheckBtnClick",
        "click #RunCompanyCheckBtn": "RunCompanyCheckBtnClick",
        "click #RunAmlCheckBtn": "RunAmlCheckBtnClick",
        "click #RunBwaCheckBtn": "RunBwaCheckBtnClick",
        "click .btn-download": "downloadConsent",
        "click .check-history tr:not(:eq(0),:eq(1))": "CheckHistoryClicked",
        "click .show-balance-history": "showHistory",
    },
    
    CheckHistoryClicked: function(e) {
        var $el = $(e.currentTarget);
        var id = $el.find("td:eq(0)").text();
        this.model.set({
            "logId": id,
            "getFromLog": true
        }, {
            silent: true
        });
        BlockUi("on");
        this.model.fetch({ data: { logId: id, getFromLog: true } }).done(function() {
            BlockUi("off");
        });
        this.render();
    },
    showHistory: function(e) {
        var hist = new EzBob.Underwriter.ExperianBalanceHistory({ history: $(e.currentTarget).data('balance') });
        EzBob.App.jqmodal.show(hist);
        $('.balance-history .inline-sparkline').sparkline("html", {
            width: "100%",
            height: "100%",
            lineWidth: 2,
            spotRadius: 3.5,
            lineColor: "#cfcfcf",
            fillColor: "transparent",
            spotColor: "#cfcfcf",
            maxSpotColor: "#cfcfcf",
            minSpotColor: "#cfcfcf",
            valueSpots: {
                ':': '#cfcfcf'
            }
        });
    },
    downloadConsent: function (e) {
        var $el = $(e.currentTarget);
        $el.attr("href", window.gRootPath + "Underwriter/CreditBureau/DownloadConsentAgreement/"+this.model.get("Id") );
    },
    RunConsumerCheckBtnClick: function (e) {
        if ($(e.currentTarget).hasClass("disabled")) return false;
        
        var customerId = this.model.get("Id");
        var directorId = 0;

        var selectedElem = $('#ConsumerCheckTarget').find('option:selected')[0];
        if (selectedElem != $('#ConsumerCheckTarget').find('option')[0]) {
            directorId = selectedElem.value;
        }

        var that = this;
        if (directorId == 0) {
            var consumer = this.model.get("Consumer");
            if (consumer.IsDataRelevant) {
                EzBob.ShowMessage("Last check was done at " + consumer.CheckDate + " and cache is valid till " + consumer.CheckValidity + ". Run check anyway?", "No need for check warning",
                    function() {
                        that.RunConsumerCheck(customerId, directorId, true);
                        return true;
                    },
                    "Yes", null, "No");
            } else {
                that.RunConsumerCheck(customerId, directorId, false);
            }
        } else {
            var director = _.find(this.model.get('Directors'), function (dir) { return dir.Id == parseInt(directorId); });
            if (director.IsDataRelevant) {
                EzBob.ShowMessage("Last check was done at " + director.CheckDate + " and cache is valid till " + director.CheckValidity + ". Run check anyway?", "No need for check warning",
                    function () {
                        that.RunConsumerCheck(customerId, directorId, true);
                        return true;
                    },
                    "Yes", null, "No");
            } else {
                that.RunConsumerCheck(customerId, directorId, false);
            }
        }
        return false;
    },
    RunConsumerCheck: function (customerId, directorId, forceCheck) {
        BlockUi("on");
        $.post(window.gRootPath + "Underwriter/CreditBureau/RunConsumerCheck", { customerId: customerId, directorId: directorId, forceCheck: forceCheck })
            .done(function (response) {
                EzBob.ShowMessage(response.Message, "Information");
            })
            .fail(function (data) {
                console.error(data.responseText);
            })
            .complete(function() {
                BlockUi("off");
            });
        return false;
    },
    RunCompanyCheckBtnClick: function (e) {
        if ($(e.currentTarget).hasClass("disabled")) return false;

        var that = this;
        BlockUi("on");
        $.post(window.gRootPath + "Underwriter/CreditBureau/IsCompanyCacheRelevant", { customerId: this.model.get("Id") })
            .done(function (response) {
                if (response.NoCompany) {
                    EzBob.ShowMessage("Customer don't have a company", "Nothing to recheck");
                } else {
                    if (response.IsRelevant == "True") {
                        EzBob.ShowMessage("Last check was done at " + response.LastCheckDate + " and cache is valid for " + response.CacheValidForDays + " days. Run check anyway?", "No need for check warning",
                            function() {
                                that.RunCompanyCheck(true);
                                return true;
                            },
                            "Yes", null, "No");
                    } else {
                        that.RunCompanyCheck(false);
                    }
                }
            })
            .complete(function () {
                BlockUi("off");
            });

        return false;
    },
    RunCompanyCheck: function (forceCheck) {
        BlockUi("on");

        $.post(window.gRootPath + "Underwriter/CreditBureau/RunCompanyCheck", { id: this.model.get("Id"), forceCheck: forceCheck })
            .done(function (response) {
                EzBob.ShowMessage(response.Message, "Information");
            })
            .fail(function (data) {
                console.error(data.responseText);
            })
            .complete(function () {
                BlockUi("off");
            });
        return false;
    },
    RunAmlCheckBtnClick: function () {
        var id = this.model.get("Id");

        var customAddressView = new EzBob.Underwriter.IdHubCustomAddressView({
            el: this.$el.find('#idhub-custom-address'),
            model: new EzBob.Underwriter.IdHubCustomAddressModel({ Id: id }),
            checkType: 1,
            dialogTitle: 'AML'
        });

        BlockUi("On");
        customAddressView.model.fetch()
            .done(function () {
                BlockUi("Off");
                customAddressView.render();
            });

        return false;
    },
    RunBwaCheckBtnClick: function () {
    var id = this.model.get("Id");

    var customAddressView = new EzBob.Underwriter.IdHubCustomAddressView({
        el: this.$el.find('#idhub-custom-address'),
        model: new EzBob.Underwriter.IdHubCustomAddressModel({ Id: id }),
        checkType: 2,
        dialogTitle: 'BWA'
    });

    BlockUi("On");
    customAddressView.model.fetch()
            .done(function () {
                BlockUi("Off");
                customAddressView.render();
            });

    return false;
    }
});

EzBob.Underwriter.ExperianBalanceHistory = Backbone.Marionette.ItemView.extend({
    template: "#balance-history-template",
    initialize: function(options) {
        this.history = options.history;
    },
    onRender: function () {
        this.$el.find('.inline-sparkline').attr('values', this.history);
    },
    jqoptions: function() {
        return {
            modal: true,
            width: 320,
            title: 'Financial Account Balance history',
            resizable: true
        };
    }
});