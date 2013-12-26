var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CustomerRelationsData = Backbone.Collection.extend({
    url: function() {
        return window.gRootPath + "Underwriter/CustomerRelations/Index/" + this.customerId;
    }
});

EzBob.Underwriter.CustomerRelationsActions = Backbone.Collection.extend({
    url: function () {
        return window.gRootPath + "Underwriter/CustomerRelations/Actions";
    }
});

EzBob.Underwriter.CustomerRelationsStatuses = Backbone.Collection.extend({
    url: function () {
        return window.gRootPath + "Underwriter/CustomerRelations/Statuses";
    }
});

EzBob.Underwriter.CustomerRelationsView = Backbone.Marionette.ItemView.extend({
    template: '#customerRelationsTemplate',
    initialize: function () {
        this.model.on("reset sync", this.render, this);
        this.actions = new EzBob.Underwriter.CustomerRelationsActions();
        this.actions.fetch();
        this.statuses = new EzBob.Underwriter.CustomerRelationsStatuses();
        this.statuses.fetch();
    },
    serializeData: function () {
        return { vals: this.model.toJSON() };
    },

    events: {
        "click .addNewCustomerRelationsEntry": "addNewCustomerRelationsEntry"
    },

    addNewCustomerRelationsEntry: function () {
        var actionsObject = {};
        for (var i = 0; i < this.actions.length; i++) {
            actionsObject[this.actions.models[i].attributes.Id] = this.actions.models[i].attributes.Name;
        }
        var statusesObject = {};
        for (var j = 0; j < this.statuses.length; j++) {
            statusesObject[this.statuses.models[j].attributes.Id] = this.statuses.models[j].attributes.Name;
        }
        var options = { actions: actionsObject, statuses: statusesObject, mainTab: this };
        var view = new EzBob.Underwriter.AddCustomerRelationsEntry(options);
        EzBob.App.jqmodal.show(view);
        return false;
    }
});