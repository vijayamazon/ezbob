var EzBob = EzBob || {};

EzBob.ZooplaView = Backbone.Marionette.View.extend({
    initialize: function (options) {
        this.template = _.template($('#zoopla-template').html());
        this.model = options.model;
    },
    render: function () {
        this.$el.html(this.template());
        this.$el.find('i[data-turnover]').tooltip({ title: 'Percentage turnover for the requested area. The Turnover is calculated by dividing the number of sales over the last 5 years (excluding new build properties) by the number of homes in a given area.' });
        this.$el.find('i[data-zoopla]').tooltip({ title: 'A link to the appropriate sold prices page on www.zoopla.co.uk' });
        return this;
    },
    jqoptions: function () {
        return {
            modal: true,
            resizable: false,
            title: "Zoopla",
            position: "center",
            draggable: false,
            width: "73%",
            height: Math.max(window.innerHeight * 0.9, 600),
            dialogClass: "zoopla"
        };
    },
});