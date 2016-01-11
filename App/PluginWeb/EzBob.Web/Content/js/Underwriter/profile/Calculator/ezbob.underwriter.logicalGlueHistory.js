var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.LogicalGlueHistoryView = Backbone.Marionette.ItemView.extend({
	template: '#logical-glue-history-template',
    initialize: function () {
    	this.model.on('change reset sync', this.render, this);
    	this.currentHistoryID = -1;
	    this.currentGuid = '';
	    return this;
    },

    events: {
    	'click .logical-glue-tr': 'rowClicked'
    },

    rowClicked: function (el) {
    	var line = $(el.currentTarget).data('line');
    	var id = $(el.currentTarget).data('id');
    	this.currentHistoryID = line;
	    this.currentGuid = id;
	    this.render();
    },

    serializeData: function() {
	    return {
	    	logicalGlue: this.model.get('LogicalGlueHistory'),
	    	currentHistoryID: this.currentHistoryID,
	    	currentGuid: this.currentGuid
	    };
    },

    onRender: function() {
		return this;
    }
});


