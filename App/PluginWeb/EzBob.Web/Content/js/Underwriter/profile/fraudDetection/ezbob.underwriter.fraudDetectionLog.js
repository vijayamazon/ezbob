var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.fraudDetectionLogModel = Backbone.Model.extend({
    model: EzBob.Underwriter.fraudDetectionLogModel,
    url: function () {
        return window.gRootPath + "Underwriter/FraudDetectionLog/Index/" + this.customerId;
    }
});

EzBob.Underwriter.IovationDetailsModel = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Underwriter/FraudDetectionLog/IovationDetails/" + this.get('Id');
    }
});

EzBob.Underwriter.IovationFraudDetailsView = Backbone.Marionette.ItemView.extend({
    template: '#iovation-details-template',
    initialize: function () {
        this.model.on("change sync", this.render, this);
        this.model.fetch();
    },
    serializeData: function () {
        return {
            iovation : this.model.toJSON()
        };
    },
    jqoptions: function () {
        return {
            modal: true,
            title: 'Iovation details',
            position: 'top',
            width: 530,
            dialogClass: 'iovation-details-popup'
        };
    }, // jqoptions
});


EzBob.Underwriter.FraudDetectionLogView = Backbone.Marionette.ItemView.extend({
    template: '#fraudDetectionLog',
    initialize: function () {
        this.model.on("change sync", this.render, this);
    },
    events: {
        "click #recheckFraud": "reCheck",
        "click .fraud-detection-row": "fraudDetectionRowClicked"
    },
    reCheck: function () {
        BlockUi('on');
        $.post(window.gRootPath + "Underwriter/FraudDetectionLog/Recheck", { customerId: this.customerId })
               .done(function () {
                   EzBob.ShowMessage("Fraud recheck Started", "refresh in a couple of minutes", null, "OK");
               })
               .always(function () {
                   BlockUi('off');
               });
    },
    fraudDetectionRowClicked: function (ev) {
        var type = $(ev.currentTarget).data('type');
        var value = $(ev.currentTarget).data('value');
        if (type === 'Iovation') {
            this.iovationDetailsModel = new EzBob.Underwriter.IovationDetailsModel({ Id: value });
            this.iovationDetailsView = new EzBob.Underwriter.IovationFraudDetailsView({ model: this.iovationDetailsModel });
            EzBob.App.jqmodal.show(this.iovationDetailsView)
        }

    },
    serializeData: function () {
        return {
            vals: this.model.get("FraudDetectionLogRows"),
            checkDate: this.model.get("LastCheckDate"),
            refNum: this.model.get('CustomerRefNumber')
        };
    }
});



