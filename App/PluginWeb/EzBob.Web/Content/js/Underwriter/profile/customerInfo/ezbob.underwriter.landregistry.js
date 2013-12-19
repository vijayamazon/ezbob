var EzBob = EzBob || {};

EzBob.LandRegistryView = Backbone.Marionette.View.extend({
    initialize: function (options) {
        this.template = _.template($('#landregistry-template').html());
        this.model = options.model;
    },
    render: function () {
        this.$el.html(this.template());
        return this;
    },
    jqoptions: function () {
        return {
            modal: true,
            resizable: false,
            title: "Land Registry",
            position: "center",
            draggable: false,
            width: "73%",
            height: Math.max(window.innerHeight * 0.9, 600),
            dialogClass: "landregistry"
        };
    },
});