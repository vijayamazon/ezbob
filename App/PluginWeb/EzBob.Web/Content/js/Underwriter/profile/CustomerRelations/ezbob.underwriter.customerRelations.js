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
        var xhr;
        if (!EzBob.CrmActions || EzBob.CrmActions.length == 0) {
            xhr = $.get(window.gRootPath + "Underwriter/CustomerRelations/CrmStatic", function(data) {
                EzBob.CrmActions = data.CrmActions;
                EzBob.CrmStatuses = data.CrmStatuses;
                EzBob.CrmRanks = data.CrmRanks;
            });
            var that = this;
            xhr.done(function() {
                that.showAddCrmPopup();
            });
        } else {
            this.showAddCrmPopup();
        }
    },
    
    showAddCrmPopup: function() {
        var view = new EzBob.Underwriter.AddCustomerRelationsEntry({ model: this.model });
        EzBob.App.jqmodal.show(view);
        return false;
    }
});