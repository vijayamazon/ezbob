var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SettingsMainView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#settings-main-template').html());
    },
    render: function () {
        this.$el.html(this.template({ 'settings': this.model.toJSON() }));
        return this;
    },
    events: {
        'click .edit-password': 'editPassword',
        'click .edit-question': 'editQuestion'
    },
    editPassword: function () {
        this.trigger('edit:password');
        return false;
    },
    editQuestion: function () {
        this.trigger('edit:question');
        return false;
    }
});