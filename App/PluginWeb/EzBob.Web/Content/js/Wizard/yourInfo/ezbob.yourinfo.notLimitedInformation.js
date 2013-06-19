var EzBob = EzBob || {};

EzBob.NonLimitedInformationView = EzBob.YourInformationStepViewBase.extend({
	initialize: function () {
		this.constructor.__super__.initialize.call(this);
		this.template = _.template($('#nonlimitededinfo-template').html());
		this.ViewName = "NonLimited";
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
	next: function () {
		var nAddressCount = 0;
		var nFilledAddressCount = 0;
		
		$('.director_address').each(function() {
			nAddressCount++;

			var oAddrInput = $(this).find('.addAddressInput:visible');

			if (oAddrInput.length)
				oAddrInput.addClass('error');
			else
				nFilledAddressCount++;
		}); // each director address

		var bAddressOk = nAddressCount == nFilledAddressCount;

		if (!this.validator.form() || !this.companyAddressValidator || !bAddressOk) {
			if (!this.companyAddressValidator)
				this.addAddressError("#NonLimitedCompanyAddress");

			if (!bAddressOk || !this.validator.form())
				EzBob.App.trigger("error", "You must fill in all of the fields.");

			return false;
		} // if

		this.trigger('next');
		return false;
	},
	render: function () {
		this.constructor.__super__.render.call(this);

		var nonLimitedAddressView = new EzBob.AddressView({ model: this.model.get('NonLimitedCompanyAddress'), name: "NonLimitedCompanyAddress", max: 1 });
		nonLimitedAddressView.render().$el.appendTo(this.$el.find('#NonLimitedCompanyAddress'));
		this.model.get('NonLimitedCompanyAddress').on("all", this.NonLimitedCompanyAddressChanged, this);

		var directorsView = new EzBob.DirectorMainView({ model: this.model.get('NonLimitedDirectors'), name: "nonlimitedDirectors", hidden: this.$el.find('.directorsData') });
		directorsView.render().$el.appendTo(this.$el.find('.directors'));

		this.$el.find(".cashInput").cashEdit();
		this.$el.find(".addressCaption").hide();

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		return this;
	},
	

	getValidator: function () {
	    return EzBob.validateNonLimitedCompanyDetailForm;
	},

	NonLimitedCompanyAddressChanged: function (evt, oModel) {
	    this.companyAddressValidator = oModel.collection.length > 0;
	    this.inputChanged();
		this.clearAddressError("#NonLimitedCompanyAddress");
	}
});