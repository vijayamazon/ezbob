var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MedalCalculationModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/Medal/Index/"
});

EzBob.Underwriter.MedalCalculationView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#medal-calculation-template').html());
        this.model.on('change reset sync', this.render, this);
    },

    events: {
        'click .export-to-exel': 'exportExcel'
    },

    exportExcel: function () {
        location.href = window.gRootPath + 'Underwriter/Medal/ExportToExel?id=' + this.model.get('Id');
    }, 
    render: function () {
        this.$el.html(this.template({ medals: this.model.toJSON() }));
        return this;
    }
});


