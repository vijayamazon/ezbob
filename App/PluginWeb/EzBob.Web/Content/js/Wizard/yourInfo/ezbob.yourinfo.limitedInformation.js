var EzBob = EzBob || {};

EzBob.LimitedInformationView = EzBob.YourInformationStepViewBase.extend({
	initialize: function () {
		this.constructor.__super__.initialize.call(this);
		this.template = _.template($('#limitedinfo-template').html());
		this.ViewName = "Limited";
		this.companyAddressValidator = false;

		this.events = _.extend({}, this.events, {
            'change input': 'inputChanged',
		    'keyup input': 'inputChanged'
		});
	},
	
	inputChanged: function () {
	    var enabled = EzBob.Validation.checkForm(this.validator) && this.companyAddressValidator;
        $('.continue').toggleClass('disabled', !enabled);
    },
	
	render: function () {
		this.constructor.__super__.render.call(this);
		var limitedAddressView = new EzBob.AddressView({ model: this.model.get('LimitedCompanyAddress').reset(), name: "LimitedCompanyAddress", max: 1 });
		limitedAddressView.render().$el.appendTo(this.$el.find('#LimitedCompanyAddress'));
		this.model.get('LimitedCompanyAddress').on("all", this.LimitedCompanyAddressChanged, this);

		var directorsView = new EzBob.DirectorMainView({ model: this.model.get('LimitedDirectors'), name: 'limitedDirectors', hidden: this.$el.find('.directorsData') });
		directorsView.render().$el.appendTo(this.$el.find('.directors'));
		this.$el.find(".cashInput").cashEdit();
		this.$el.find(".addressCaption").hide();
	},
	
	getValidator: function () {
	    return EzBob.validateLimitedCompanyDetailForm;
	},

	next: function (e) {
	    var $el = $(e.currentTarget);
	    if ($el.hasClass("disabled")) return false;
		this.trigger('next');
		return false;
	},
	LimitedCompanyAddressChanged: function (evt, oModel) {
	    this.companyAddressValidator = oModel.collection && oModel.collection.length > 0;
	    this.inputChanged();
		this.clearAddressError("#LimitedCompanyAddress");
	}
});