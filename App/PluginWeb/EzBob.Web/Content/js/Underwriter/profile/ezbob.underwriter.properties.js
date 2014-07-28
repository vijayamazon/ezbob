var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.Properties = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Underwriter/Properties/Index/" + this.customerId;
    }
});

EzBob.Underwriter.PropertiesView = Backbone.Marionette.ItemView.extend({
    template: '#propertiesTemplate',
    initialize: function () {
        this.model.on("reset sync", this.render, this);
    },
    serializeData: function () {
        return { vals: this.model.toJSON() };
    }
});

