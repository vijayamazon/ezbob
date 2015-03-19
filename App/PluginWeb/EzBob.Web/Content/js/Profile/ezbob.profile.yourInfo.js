var EzBob;

EzBob = EzBob || {};

EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.YourInfoMainView = Backbone.Marionette.Layout.extend({
	template: '#your-info-template',

	initialize: function () {
		return EzBob.App.on('dash-director-address-change', this.directorModelChange, this);
	},

	events: {
		'click .edit-personal': 'editPersonalViewShow',
		'click .submit-personal': 'saveData',
		'click .cancel': 'reload',
		'change .personEditInput': 'inputChanged',
		'keyup .personEditInput': 'inputChanged'
	},

	ui: {
		form: 'form.editYourInfoForm'
	},

	setInputReadOnly: function (isReadOnly) {
		var isOwnerOfOtherProperties, otherPropertiesModels;
		this.$el.find('.personEditInput').attr('readonly', isReadOnly);
		this.$el.find('#PersonalAddress .addAddressInput');
		this.$el.find('#OtherPropertiesAddresses .addAddressInput');
		isOwnerOfOtherProperties = this.model.get('PropertyStatus').IsOwnerOfOtherProperties;
		otherPropertiesModels = this.model.get('OtherPropertiesAddresses');
		if (isReadOnly) {
			this.$el.find('.submit-personal, .cancel,#PersonalAddress .addAddressInput,#PersonalAddress .addAddress,#PersonalAddress .removeAddress,#PersonalAddress .attardi-input,#PersonalAddress .required').hide();
			if (!isOwnerOfOtherProperties || otherPropertiesModels === void 0 || otherPropertiesModels.length < 1) {
				this.$el.find('#otherPropertiesDiv').hide();
			} else {
				this.$el.find('#otherPropertiesDiv .removeAddress, #otherPropertiesDiv .addAddressContainer').hide();
			}
			this.$el.find('textarea').removeClass('form_field').css('margin-top', 0);
			return this.$el.find('.edit-personal').show();
		} else {
			this.$el.find('.submit-personal, .cancel,#PersonalAddress .removeAddress').show();
			if (isOwnerOfOtherProperties && (otherPropertiesModels === void 0 || otherPropertiesModels.length < 3)) {
				this.$el.find('#otherPropertiesDiv, #otherPropertiesDiv .addAddressContainer').show();
			}
			return this.$el.find('.edit-personal').hide();
		}
	},
	editPersonalViewShow: function () {
		return this.setInputReadOnly(false);
	},
	onAddingDirector: function () {
		return this.ui.form.hide();
	},
	onDirectorAdded: function () {
		this.ui.form.hide();
		return this.reload();
	},
	onBackFromDirector: function () {
		return this.ui.form.show();
	},
	addressAreValid: function () {
		var address, dir, directors, typeOfBusinessName, _i, _len;
		address = this.model.get('PersonalAddress');
		if (address.length < 1) {
			return false;
		}
		typeOfBusinessName = this.model.get('BusinessTypeReduced');
		if (typeOfBusinessName === 'Limited') {
			address = this.model.get('CompanyAddress');
			if (address.length < 1) {
				return false;
			}
		} else if (typeOfBusinessName === 'NonLimited') {
			address = this.model.get('CompanyAddress');
			if (address.length < 1) {
				return false;
			}
		}
		if (this.model.get(typeOfBusinessName + 'Info')) {
			directors = this.model.get(typeOfBusinessName + 'Info').Directors;
			for (_i = 0, _len = directors.length; _i < _len; _i++) {
				dir = directors[_i];
				if (dir.DirectorAddress.length < 1) {
					return false;
				}
			}
		}
		return true;
	},
	directorModelChange: function (newModel) {
		var directors;
		directors = this.model.get(this.model.get('BusinessTypeReduced') + 'Info').Directors;
		_.each(directors, function (dir) {
			if (dir.Id === newModel.get('Id')) {
				return dir.DirectorAddress = newModel.get('DirectorAddress').models;
			}
		});
		return this.addressModelChange();
	},
	addressModelChange: function () {
		var directors, self, typeOfBusinessName;
		this.inputChanged();
		this.setInvalidAddressLabel(this.model.get('PersonalAddress'), '#PersonalAddress');
		typeOfBusinessName = this.model.get('BusinessTypeReduced');
		if (typeOfBusinessName === 'Limited') {
			this.setInvalidAddressLabel(this.model.get('CompanyAddress'), '#LimitedCompanyAddress');
		} else if (typeOfBusinessName === 'NonLimited') {
			this.setInvalidAddressLabel(this.model.get('CompanyAddress'), '#NonLimitedAddress');
		}
		self = this;
		if (this.model.get(typeOfBusinessName + 'Info')) {
			directors = this.model.get(typeOfBusinessName + 'Info').Directors;
			return _.each(directors, function (dir) {
				return self.setInvalidAddressLabel(dir.DirectorAddress, '.directorAddress' + dir.Id + ' #DirectorAddress', dir.Id);
			});
		}
	},
	setInvalidAddressLabel: function (address, element, dirId) {
		if (address.length < 1) {
			return EzBob.Validation.addressErrorPlacement(this.$el.find(element), (dirId ? this.model : address), dirId, this.model.get('BusinessTypeReduced'));
		} else {
			return EzBob.Validation.unhighlight(this.$el.find(element));
		}
	},
	saveData: function () {
		var action, data, directors, request, typeOfBusinessName;
		if (!this.validator.form() || !this.addressAreValid()) {
			EzBob.App.trigger('error', 'You must fill in all of the fields.');
			return false;
		}
		typeOfBusinessName = this.model.get('BusinessTypeReduced') + 'Info';
		if (this.model.get(typeOfBusinessName)) {
			directors = this.model.get(typeOfBusinessName).Directors;
			_.each(directors, function (val) {
				return _.each(val.DirectorAddress, function (add) {
					return add['DirectorId'] = val.Id;
				});
			});
		}
		data = this.ui.form.serializeArray();
		action = this.ui.form.attr('action');
		var self = this;
		request = $.post(action, data);
		request.done(function () {
			self.reload();
			EzBob.App.trigger('info', 'Your information updated successfully');
		});

		request.fail(function () {
			EzBob.App.trigger('error', 'Business check service temporary unavaliable, please contact with system administrator', '');
		});

		return false;
	},
	reload: function () {
		var self = this;
		this.model.fetch().done(function () {
			self.render();
			scrollTop();
			self.setInputReadOnly(true);
		});
	},
	regions: {
		personal: '.personal-info',
		company: '.company-info'
	},
	inputChanged: function () {
		var isInvalid;
		isInvalid = !this.validator.form() || !this.addressAreValid();
		return this.$el.find('.submit-personal').toggleClass('disabled', isInvalid).prop('disabled', isInvalid);
	},
	onRender: function () {
		this.renderPersonal();

		var typeOfBusinessName = this.model.get('BusinessTypeReduced');

		if (typeOfBusinessName === 'Limited') {
			this.renderLimited();
		} else if (typeOfBusinessName === 'NonLimited') {
			this.renderNonLimited();
		}

		this.setInputReadOnly(true);
		this.validator = EzBob.validateYourInfoEditForm(this.ui.form);
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.cashInput').numericOnly(15);
		$('input.form_field_right_side').css('margin-left', '3px');
		return EzBob.UiAction.registerView(this);
	},
	renderPersonal: function () {
		var personalInfoView = new EzBob.Profile.PersonalInfoView({ model: this.model });
		this.model.get('PersonalAddress').on('all', this.addressModelChange, this);
		this.personal.show(personalInfoView);
	},
	renderNonLimited: function () {
		var view = new EzBob.Profile.NonLimitedInfoView({
			model: this.model,
			parentView: this
		});
		this.model.get('CompanyAddress').on('all', this.addressModelChange, this);
		return this.company.show(view);
	},
	renderLimited: function () {
		var view = new EzBob.Profile.LimitedInfoView({
			model: this.model,
			parentView: this
		});
		this.model.get('CompanyAddress').on('all', this.addressModelChange, this);
		return this.company.show(view);
	}
});

EzBob.Profile.PersonalInfoView = Backbone.Marionette.Layout.extend({
	template: '#personal-info-template',
	regions: {
		personAddress: '#PersonalAddress',
		otherPropertiesAddresses: '#OtherPropertiesAddresses'
	},
	onRender: function () {
		var address, otherPropertiesAddressesView;
		address = new EzBob.AddressView({
			model: this.model.get('PersonalAddress'),
			name: 'PersonalAddress',
			max: 10,
			isShowClear: true,
			uiEventControlIdPrefix: this.personAddress.getEl(this.personAddress.el).attr('data-ui-event-control-id-prefix')
		});
		this.personAddress.show(address);
		otherPropertiesAddressesView = new EzBob.AddressView({
			model: this.model.get('OtherPropertiesAddresses'),
			name: 'OtherPropertiesAddresses',
			max: 3,
			required: "empty",
			isShowClear: false,
			uiEventControlIdPrefix: this.otherPropertiesAddresses.getEl(this.otherPropertiesAddresses.el).attr('data-ui-event-control-id-prefix')
		});
		this.otherPropertiesAddresses.show(otherPropertiesAddressesView);
		return this;
	}
});

EzBob.Profile.NonLimitedInfoView = Backbone.Marionette.Layout.extend({
	template: '#nonlimited-info-template',
	initialize: function (options) {
		this.parentView = options.parentView;
		return this.lastTimeDupDirFound = false;
	},
	regions: {
		nonlimitedAddress: '#NonLimitedAddress',
		director: '.director-container'
	},
	events: {
		"click .add-director": "addDirectorClicked"
	},
	onRender: function () {
		var address, directorView, directors;
		address = new EzBob.AddressView({
			model: this.model.get('CompanyAddress'),
			name: 'NonLimitedCompanyAddress',
			max: 10,
			isShowClear: true,
			uiEventControlIdPrefix: this.nonlimitedAddress.getEl(this.nonlimitedAddress.el).attr('data-ui-event-control-id-prefix')
		});
		this.nonlimitedAddress.show(address);
		directors = this.model.get('CompanyInfo').Directors;
		if (directors !== null && directors.length !== 0) {
			directorView = new EzBob.Profile.DirectorCompositeView({
				collection: new EzBob.Directors(directors)
			});
			this.director.show(directorView);
		}
		if (!this.model.get('IsOffline')) {
			this.$el.find('.offline').remove();
		} else {
			this.$el.find('.notoffline').remove();
		}
		return this;
	},
	addDirectorClicked: function (event) {
		var customerInfo, director, directorEl;
		event.stopPropagation();
		event.preventDefault();
		this.parentView.onAddingDirector();
		director = new EzBob.DirectorModel();
		directorEl = $('.add-director-container');
		customerInfo = _.extend({}, this.model.get('CustomerPersonalInfo'), {
			PostCode: this.model.get('PersonalAddress').models[0].get('Rawpostcode')
		}, {
			Directors: this.model.get('CompanyInfo').Directors
		});
		if (!this.addDirector) {
			this.addDirector = new EzBob.AddDirectorInfoView({
				model: director,
				el: directorEl,
				customerInfo: customerInfo,
				failOnDuplicate: true
			});

			var self = this;
			this.addDirector.setBackHandler(function () {
				directorEl.hide();
				self.parentView.onBackFromDirector();
			});
			
			this.addDirector.setSuccessHandler(function () {
				directorEl.hide();
				self.parentView.onDirectorAdded();
			});
			
			this.addDirector.setDupCheckCompleteHandler(function (bDupFound) {
				if (bDupFound) {
					if (!self.lastTimeDupDirFound) {
						EzBob.App.trigger('clear');
						EzBob.App.trigger('error', 'Duplicate director detected.');
					}
					return self.lastTimeDupDirFound = true;
				} else {
					EzBob.App.trigger('clear');
					return self.lastTimeDupDirFound = false;
				}
			});
			
			this.addDirector.render();
		}
		directorEl.show();
		return false;
	}
});

EzBob.Profile.LimitedInfoView = Backbone.Marionette.Layout.extend({
	template: '#limited-info-template',
	initialize: function (options) {
		this.parentView = options.parentView;
		return this.lastTimeDupDirFound = false;
	},
	regions: {
		limitedAddress: '#LimitedCompanyAddress',
		director: '.director-container'
	},
	events: {
		"click .add-director": "addDirectorClicked"
	},
	onRender: function () {
		var address, directorView, directors;
		address = new EzBob.AddressView({
			model: this.model.get('CompanyAddress'),
			name: 'LimitedCompanyAddress',
			max: 10,
			isShowClear: true,
			uiEventControlIdPrefix: this.limitedAddress.getEl(this.limitedAddress.el).attr('data-ui-event-control-id-prefix')
		});
		this.limitedAddress.show(address);
		directors = this.model.get('CompanyInfo').Directors;
		if (directors !== null && directors.length !== 0) {
			directors = _.filter(directors, function (director) {
				return !director.IsExperianDirector && !director.IsExperianShareholder;
			});
			directorView = new EzBob.Profile.DirectorCompositeView({
				collection: new EzBob.Directors(directors)
			});
			this.director.show(directorView);
		}
		if (!this.model.get('IsOffline')) {
			this.$el.find('.offline').remove();
		}
		return this;
	},
	addDirectorClicked: function (event) {
		var customerInfo, director, directorEl;
		event.stopPropagation();
		event.preventDefault();
		this.parentView.onAddingDirector();
		director = new EzBob.DirectorModel();
		directorEl = $('.add-director-container');
		customerInfo = _.extend({}, this.model.get('CustomerPersonalInfo'), {
			PostCode: this.model.get('PersonalAddress').models[0].get('Rawpostcode')
		}, {
			Directors: this.model.get('CompanyInfo').Directors
		});
		if (!this.addDirector) {
			this.addDirector = new EzBob.AddDirectorInfoView({
				model: director,
				el: directorEl,
				customerInfo: customerInfo,
				failOnDuplicate: true
			});

			var self = this;
			this.addDirector.setBackHandler(function () {
				directorEl.hide();
				self.parentView.onBackFromDirector();
			});
			
			this.addDirector.setSuccessHandler(function () {
				directorEl.hide();
				self.parentView.onDirectorAdded();
			});
			
			this.addDirector.setDupCheckCompleteHandler(function (bDupFound) {
				if (bDupFound) {
					if (!self.lastTimeDupDirFound) {
						EzBob.App.trigger('clear');
						EzBob.App.trigger('error', 'Duplicate director detected.');
					}
					return self.lastTimeDupDirFound = true;
				} else {
					EzBob.App.trigger('clear');
					return self.lastTimeDupDirFound = false;
				}
			});
			
			this.addDirector.render();
		}
		directorEl.show();
		return false;
	}
});

EzBob.Profile.DirectorInfoView = Backbone.Marionette.Layout.extend({
	template: '#director-info-template',
	regions: {
		directorAddress: '#DirectorAddress'
	},
	addressModelChange: function () {
		return EzBob.App.trigger('dash-director-address-change', this.model);
	},
	onRender: function () {
		var address;
		address = new EzBob.AddressView({
			model: this.model.get('DirectorAddress'),
			name: "DirectorAddress[" + (this.model.get('Position')) + "]",
			max: 10,
			isShowClear: true,
			directorId: this.model.get('Id'),
			uiEventControlIdPrefix: this.directorAddress.getEl(this.directorAddress.el).attr('data-ui-event-control-id-prefix')
		});
		this.model.get('DirectorAddress').on('all', this.addressModelChange, this);
		this.directorAddress.show(address);
		return this.$el.find('.addressEdit').addClass('directorAddress' + this.model.get('Id'));
	}
});

EzBob.Profile.DirectorCompositeView = Backbone.Marionette.CompositeView.extend({
	template: '#directors-info',
	itemView: EzBob.Profile.DirectorInfoView,
	itemViewContainer: 'div'
});
