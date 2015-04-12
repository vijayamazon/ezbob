var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AutomationCalculationModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/Automation/Index/"
});

EzBob.Underwriter.AutomationCalculationView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#automation-calculation-template').html());
        this.model.on('change reset sync', this.render, this);

        this.currentId = 0;
    },

    events: {
        'change #AutomationDetailsHistory': 'changeHistory'
    },

    changeHistory: function(el) {
        this.currentId = $(el.currentTarget).val();
        this.render();
    },
    
    render: function() {
		
        var automations = this.model.toJSON();
        var current = (automations.trails && automations.trails.length > 0) ? automations.trails[this.currentId] : {};

        this.$el.html(this.template({
            automations: automations,
            current: current,
            currentId: this.currentId
        }));

        return this;
    }
});


