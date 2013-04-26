var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.fraudDetectionLogModel = Backbone.Model.extend({

});

EzBob.Underwriter.FraudDetectionLogs = Backbone.Collection.extend({
    model: EzBob.Underwriter.fraudDetectionLogModel,
    url: function () {
        return window.gRootPath + "Underwriter/FraudDetectionLog/Index/" + this.customerId;
    }
});

EzBob.Underwriter.FraudDetectionLogView = Backbone.Marionette.ItemView.extend({
    template: '#fraudDetectionLog',
    initialize: function () {
        this.model.on("reset", this.render, this);
    },
    serializeData: function () {
        return { vals: this.model.toJSON() };
    }
});
