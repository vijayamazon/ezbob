var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.AgreementViewBase = Backbone.Marionette.ItemView.extend({
	
    initialize: function () {
    	this.onTabSwitch = this.options.onTabSwitch;
    	this.template = _.template($(this.getTemplate()).html());
    }, // initialize
    getAgreementType: function() {
	    var type = 'ezbob';
	    if (this.options.isAlibaba) {
		    type = 'alibaba';
	    }
	    if (this.options.isEverline) {
		    type = 'everline';
	    }
	    if (this.options.isEverlineRefinance) {
	    	type = 'everlineRefinance';
	    }
	    
	    return type;
    }, //serializeData
    render: function(data) {
    	data.agreementType = this.getAgreementType();
	    var temp = Handlebars.compile(this.template(data));
    	this.$el.html(temp(data));

        this.addScroll();

        var self = this;

        this.$el.find("a[data-toggle=\"tab\"]").on("shown", function () {
            if (self.onTabSwitch)
                self.onTabSwitch();

            return self.addScroll();
        });

        EzBob.UiAction.registerView(this);

        return this;
    }, // render

    addScroll: function () {
        return this.$el.find(".overview").jScrollPane();
    }, // addScroll
}); // EzBob.Profile.AgreementViewBase

EzBob.Profile.CompaniesAgreementView = EzBob.Profile.AgreementViewBase.extend({
	getTemplate: function() {
        return "#companies-agreement-template";
    }, // getTemplate
}); // EzBob.Profile.CompaniesAgreementView

EzBob.Profile.ConsumersAgreementView = EzBob.Profile.AgreementViewBase.extend({
	getTemplate: function() {
		return "#consumers-agreement-template";
    }, // getTemplate

    render: function (data) {
        $(".company-preAgreement").hide();
        $(".consumer-preAgreement").show();
        EzBob.Profile.ConsumersAgreementView.__super__.render.call(this, data);
    }, // render
}); // EzBob.Profile.ConsumersAgreementView

