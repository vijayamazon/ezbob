var EzBob = EzBob || {};

EzBob.NonLimitedInformationView = EzBob.YourInformationStepViewBase.extend({
	initialize: function () {
		this.constructor.__super__.initialize.call(this);
		this.template = _.template($('#nonlimitededinfo-template').html());
		this.ViewName = "NonLimited";
		this.companyAddressValidator = false;
		this.events = _.extend({}, this.events, {
			'change input[name="NonLimitedCompanyName"]': 'nonLimitedCompanyNameChanged',
			'change input[name="NonLimitedBusinessPhone"]': 'nonLimitedBusinessPhoneChanged',
			'change select[name="NonLimitedTimeAtAddress"]': "nonLimitedTimeAtAddressChanged",
			'change select[name="NonLimitedTimeInBusiness"]': "nonLimitedTimeInBusinessChanged"
		});
	},
	nonLimitedCompanyNameChanged: function () {
		EzBob.Validation.displayIndication(this.validator, "NonLimitedCompanyNameImage", "#NonLimitedCompanyName");
	},
	nonLimitedBusinessPhoneChanged: function () {
		EzBob.Validation.displayIndication(this.validator, "NonLimitedBusinessPhoneImage", "#NonLimitedBusinessPhone");
	},
	nonLimitedTimeAtAddressChanged: function () {
		EzBob.Validation.displayIndication(this.validator, "NonLimitedTimeAtAddressImage", "#NonLimitedTimeAtAddress");
	},
	nonLimitedTimeInBusinessChanged: function () {
		EzBob.Validation.displayIndication(this.validator, "NonLimitedTimeInBusinessImage", "#NonLimitedTimeInBusiness");
	},
	next: function () {
		var nAddressCount = 0;
		var nFilledAddressCount = 0;

		$('.director_address').each(function() {
			nAddressCount++;

			var oAddrInput = $(this).find('.addAddressInput');

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
	NonLimitedCompanyAddressChanged: function (evt, oModel) {
		this.companyAddressValidator = oModel.collection.length > 0;
		this.clearAddressError("#NonLimitedCompanyAddress");
	}
});