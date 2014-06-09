var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.CompanyDirectorsView = Backbone.Marionette.ItemView.extend({
    template: "#company-directors-template",

    initialize: function () {

    },
    onRender: function () {

    },
    
    serializeData: function () {
        var company = this.model.get("CompanyInfo") || {};
        return {
            data: company.Directors || []
        };
    }
});