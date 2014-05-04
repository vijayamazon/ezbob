var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PricingModelCalculationModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/PricingModelCalculations/Index/"
});

EzBob.Underwriter.PricingModelCalculationView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#pricing-model-calculation-template').html());
        this.model.on('change reset sync', this.render, this);
    },

    render: function () {
        this.$el.html(this.template({ model: this.model.toJSON() }));
        return this;
    }
});


