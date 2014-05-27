var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CustomerRelationsModel = Backbone.Collection.extend({
    url: function () {
        return window.gRootPath + "Underwriter/CustomerRelations/Index/" + this.customerId;
    }
});

EzBob.Underwriter.CustomerRelationsView = Backbone.Marionette.ItemView.extend({
    template: '#customerRelationsTemplate',
    initialize: function () {
        this.model.on("change reset sync", this.render, this);
    },
    serializeData: function () {
        return { vals: this.model.toJSON() };
    },
    events: {
        "click .addNewCustomerRelationsEntry": "addNewCustomerRelationsEntry"
    },

    addNewCustomerRelationsEntry: function () {
        var view = new EzBob.Underwriter.AddCustomerRelationsEntry({model : this.model});
        EzBob.App.jqmodal.show(view);
        return false;
    }
});