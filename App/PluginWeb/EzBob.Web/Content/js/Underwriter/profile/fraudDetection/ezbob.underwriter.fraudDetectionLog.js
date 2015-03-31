var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.fraudDetectionLogModel = Backbone.Model.extend({
    model: EzBob.Underwriter.fraudDetectionLogModel,
    url: function () {
        return window.gRootPath + "Underwriter/FraudDetectionLog/Index/" + this.customerId;
    }
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
            BlockUi('on');
            $.get(window.gRootPath + "Underwriter/FraudDetectionLog/IovationDetails", { id: value })
             .done(function (response) {
                 console.log('response');
             })
             .always(function () { BlockUi('off'); });
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
