var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.AgreementView = Backbone.Marionette.ItemView.extend({
    initialize: function (options) {
        this.onTabSwitch = options.onTabSwitch;
        this.templates = options.templates;
        this.template = _.template($('#agreement-template').html());
    }, // initialize
    
    render: function (data) {
	    var temp = Handlebars.compile(this.template({ templates: this.templates, data: data }));
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
    	return this.$el.find(".overview").jScrollPane({ verticalDragMinHeight: 40 });
    }, // addScroll
}); // EzBob.Profile.AgreementViewBase
