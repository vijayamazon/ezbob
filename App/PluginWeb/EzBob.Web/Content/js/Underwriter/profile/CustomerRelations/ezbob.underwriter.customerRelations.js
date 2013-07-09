var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};


EzBob.Underwriter.CustomerRelationsData = Backbone.Collection.extend({
    url: function () {
        return window.gRootPath + "Underwriter/CustomerRelations/Index/" + this.customerId;
    }
});

EzBob.Underwriter.CustomerRelationsView = Backbone.Marionette.ItemView.extend({
    template: '#customerRelationsTemplate',
    initialize: function () {
        this.model.on("reset", this.render, this);
    },
    serializeData: function () {
        return { vals: this.model.toJSON() };
    }
});