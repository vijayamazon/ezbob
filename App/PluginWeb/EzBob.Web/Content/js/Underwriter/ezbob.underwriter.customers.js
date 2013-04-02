var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CustomersView = Backbone.View.extend({
    initialize: function (options) {
        this.grids = options.grids;
    },
    render: function () {
        this.$el.find('a[data-toggle="tab"]').on('shown', _.bind(this.tabShown, this));
        _.each(this.grids, function (g) {
            EzBob.Underwriter.customerGrid(g);
        });        
    },
    tabShown: function (e) {
        document.location.href = e.target.getAttribute('href');
    },
    show: function (type) {
        this.$el.show();
        if (type) {
            this.$el.find('.tab-pane').hide().removeClass('.active');
            this.$el.find('#' + type).show().addClass('.active');
        }
    },
    hide: function () {
        this.$el.hide();
    }
});