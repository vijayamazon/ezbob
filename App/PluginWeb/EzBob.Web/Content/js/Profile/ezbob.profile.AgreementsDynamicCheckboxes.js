var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.AgreementsDynamicCheckboxes = Backbone.Marionette.ItemView.extend({
    template: '#dynamic-agreements-checkboxes-template',
    initialize: function (options) {
        this.templates = options.templates;
        return this;
    }, // initialize
    
    onRender: function () {
        return this;
    }, // render

    serializeData: function() {
        return {
            templates: this.templates
        };
    }
    
}); // EzBob.Profile.AgreementsDynamicCheckboxes
