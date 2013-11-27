var EzBob = EzBob || {};

EzBob.LimitedInformationView = EzBob.YourInformationStepViewBase.extend({
	initialize: function() {
		this.constructor.__super__.initialize.call(this);
		this.template = _.template($('#limitedinfo-template').html());
		this.ViewName = "Limited";
		this.companyAddressValidator = false;

		this.parentView = this.options.parentView;

		this.events = _.extend({}, this.events, {
			'change #LimitedPropertyOwnedByCompany': 'propertyOwnedByCompanyChanged',
		});
	},

	readyToContinue: function() {
		return this.companyAddressValidator &&
			this.directorsView.validateAddresses() &&
			(!this.employeeCountView || this.employeeCountView.isValid());
	}, // readyToContinue

	inputChanged: function() {
		this.parentView.inputChanged();
	}, // inputChanged

	propertyOwnedByCompanyChanged: function(event) {
		var toToggle = this.$el.find('#LimitedPropertyOwnedByCompany').val() !== 'false';
		this.$el.find('.additionalCompanyAddressQuestions').toggleClass('hide', toToggle);
		this.inputChanged(event);
	},

	render: function() {
		var self = this;
		this.constructor.__super__.render.call(this);

		var limitedAddressView = new EzBob.AddressView({ model: this.model.get('LimitedCompanyAddress').reset(), name: "LimitedCompanyAddress", max: 1 });
		limitedAddressView.render().$el.appendTo(this.$el.find('#LimitedCompanyAddress'));
		this.model.get('LimitedCompanyAddress').on("all", this.LimitedCompanyAddressChanged, this);
		this.addressErrorPlacement(limitedAddressView.$el, limitedAddressView.model);

		this.directorsView = new EzBob.DirectorMainView({ model: this.model.get('LimitedDirectors'), name: 'limitedDirectors', });
		this.directorsView.on("director:change", this.inputChanged, this);
		this.directorsView.on("director:addressChanged", this.inputChanged, this);
		this.directorsView.render().$el.appendTo(this.$el.find('.directors'));

		if (this.model.get('IsOffline')) {
			this.employeeCountView = new EzBob.EmployeeCountView({
				model: this.model,
				onchange: $.proxy(self.inputChanged, self),
				prefix: "Limited"
			});
			this.employeeCountView.render().$el.appendTo(this.$el.find('.employee-count'));
		}
		else {
			this.$el.find('.offline').remove();
			this.employeeCountView = null;
		} // if

		this.$el.find(".addressCaption").hide();

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		EzBob.UiAction.registerView(this);

		return this;
	},

	getValidator: function() {
		return EzBob.validateLimitedCompanyDetailForm;
	},

	next: function(e) {
		var $el = $(e.currentTarget);

		if ($el.hasClass("disabled"))
			return false;

		this.trigger('next');
		return false;
	},

	LimitedCompanyAddressChanged: function(evt, oModel) {
		this.companyAddressValidator = oModel.collection && oModel.collection.length > 0;
		this.inputChanged();
		this.clearAddressError("#LimitedCompanyAddress");
	}
});