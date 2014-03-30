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
        this.$el.html(this.template({ experianInfo: this.model.toJSON() }));
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
    downloadConsent: function (e) {
        var $el = $(e.currentTarget);
        $el.attr("href", window.gRootPath + "Underwriter/CreditBureau/DownloadConsentAgreement/"+this.model.get("Id") );
    },
    RunConsumerCheckBtnClick: function (e) {
        if ($(e.currentTarget).hasClass("disabled")) return false;

        var that = this;
        BlockUi("on");
        $.post(window.gRootPath + "Underwriter/CreditBureau/IsConsumerCacheRelevant", { customerId: this.model.get("Id") })
            .done(function (response) {
                if (response.IsRelevant == "True") {
                    EzBob.ShowMessage("Last check was done at " + response.LastCheckDate +" and cache is valid for " + response.CacheValidForDays + " days. Run check anyway?", "No need for check warning",
                        function () {
                            that.RunConsumerCheck();
                            return true;
                        },
                        "Yes", null, "No");
                } else {
                    that.RunConsumerCheck();
                }
            })
            .complete(function () {
                BlockUi("off");
            });

        return false;
    },
    RunConsumerCheck: function () {
        BlockUi("on");
        $.post(window.gRootPath + "Underwriter/CreditBureau/RunConsumerCheck", { customerId: this.model.get("Id") })
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
                if (response.IsRelevant == "True") {
                    EzBob.ShowMessage("Last check was done at " + response.LastCheckDate + " and cache is valid for " + response.CacheValidForDays + " days. Run check anyway?", "No need for check warning",
                        function () {
                            that.RunCompanyCheck();
                            return true;
                        },
                        "Yes", null, "No");
                } else {
                    that.RunCompanyCheck();
                }
            })
            .complete(function () {
                BlockUi("off");
            });

        return false;
    },
    RunCompanyCheck: function () {
        BlockUi("on");
        $.post(window.gRootPath + "Underwriter/CreditBureau/RunCompanyCheck", { id: this.model.get("Id") })
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
