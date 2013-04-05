var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ApiChecksLogModel = Backbone.Model.extend({

});

EzBob.Underwriter.ApiChecksLogs = Backbone.Collection.extend({
    model: EzBob.Underwriter.MarketPlaceDetailModel,
    url: function () {
        return window.gRootPath + "Underwriter/ApiChecksLog/Index/" + this.customerId;
    }
});

EzBob.Underwriter.ApiChecksLogView = Backbone.Marionette.ItemView.extend({
    template: '#apiChecksLog',
    initialize: function () {
        this.model.on("reset", this.render, this);
    },
    serializeData: function () {
        return { vals: this.model.toJSON() };
    }
});