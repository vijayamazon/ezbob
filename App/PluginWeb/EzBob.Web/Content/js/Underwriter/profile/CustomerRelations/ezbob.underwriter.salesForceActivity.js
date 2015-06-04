var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SalesForceActivityModel = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Underwriter/CustomerRelations/SalesForceActivity/" + this.customerId;
    }
});

EzBob.Underwriter.SalesForceActivityView = Backbone.Marionette.ItemView.extend({
	template: '#sales-force-activity-template',

    initialize: function () {
        this.model.on("change reset sync", this.render, this);
    },

    serializeData: function () {
    	return {
    		activities: this.model.get('Activities')
    	}
    },

    onRender: function () {
	    var error = this.model.get('Error');
	    if (error) {
		    EzBob.ShowMessage(error, 'Failed to retrieve activities from SF');
	    }
    }
});

