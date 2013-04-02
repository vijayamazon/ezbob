/// <reference path="../lib/backbone.js" />
/// <reference path="../lib/underscore.js" />

var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.AccountSummaryView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#ProfileSummary').html());
        this.model.on('change', this.render, this);
    },
    render: function () {
        this.$el.html(this.template(this.model.toJSON()));
        return this;
    }
});