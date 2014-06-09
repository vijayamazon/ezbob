var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.CompanyDirectorsView = Backbone.Marionette.ItemView.extend({
    template: "#company-directors-template",

    initialize: function () {

    },
    onRender: function () {

    },
    
    serializeData: function () {
        console.log('this.model',this.model);
        return {
            data: this.model.get("CompanyInfo").Directors
        };
    }
});