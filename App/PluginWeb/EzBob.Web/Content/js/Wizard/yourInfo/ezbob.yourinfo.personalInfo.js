﻿var EzBob = EzBob || {};

EzBob.PersonalInformationStepModel = EzBob.WizardStepModel.extend({});

EzBob.PersonalInformationStepView = EzBob.YourInformationStepViewBase.extend({
	initialize: function() {
		this.template = _.template($('#personinfo-template').html());
		this.ViewName = 'personal';

		this.events = _.extend({}, this.events, {
			'change #TimeAtAddress': 'personalTimeAtAddressChanged',
			'change #ResidentialStatus': 'residentialStatusChanged',
			'click label[for="ConsentToSearch"] a': 'showConsent',

			'change input': 'inputChanged',
			'focusout input': 'inputChanged',
			'keyup input': 'inputChanged',

			'focus select': 'inputChanged',
			'focusout select': 'inputChanged',
			'keyup select': 'inputChanged',
			'click select': 'inputChanged'
		}); // events

		this.readyToProceed = false;

		this.constructor.__super__.initialize.call(this);
	}, // initialize

	showConsent: function() {
		var consentAgreementModel = new EzBob.ConsentAgreementModel({
			id: this.model.get('Id'),
			firstName: this.$el.find('#FirstName').val(),
			middleInitial: this.$el.find('#MiddleInitial').val(),
			surname: this.$el.find('#Surname').val()
		});

		var consentAgreement = new EzBob.ConsentAgreement({ model: consentAgreementModel });
		EzBob.App.modal.show(consentAgreement);
		return false;
	}, // showConsent

	inputChanged: function(event) {
		var el = event ? $(event.currentTarget) : null;

		if (el && el.attr('id') === 'MiddleInitial' && el.val() === '') {
			var img = el.closest('div').find('.field_status');
			img.field_status('set', 'empty', 2);
		} // if

		var enabled = EzBob.Validation.checkForm(this.validator) &&
			this.isPrevAddressValid() &&
			this.isAddressValid();

		$('.btn-continue').toggleClass('disabled', !enabled);
	}, // inputChanged

	isAddressValid: function() {
		var oAddress = this.model.get('PersonalAddress');
		return oAddress && (oAddress.length > 0);
	}, // isAddressValid

	residentialStatusChanged: function() {
		var oCombo = this.$el.find('#OwnOtherProperty');

		if (oCombo.length < 1)
			return;

		if (this.$el.find('#ResidentialStatus').val() === 'Home owner') {
			this.$el.find('#OwnOtherPropertyQuestion').text('Do you own another property?');
			oCombo.find('option[value="Yes"]').text('Yes, I own another property.');
			oCombo.find('option[value="No"]').text('No, I don\'t own another property.');
		}
		else {
			this.$el.find('#OwnOtherPropertyQuestion').text('Do you own a property?');
			oCombo.find('option[value="Yes"]').text('Yes, I own a property.');
			oCombo.find('option[value="No"]').text('No, I don\'t own a property.');
		} // if
	}, // residentialStatusChanged

	personalTimeAtAddressChanged: function() {
		var buttonContainer = '#PrevPersonAddresses';

		this.clearAddressError(buttonContainer);
		this.$el.find('#PrevPersonAddresses .addAddressContainer label.attardi-input span').text('Enter previous postcode');
		var jqElem = this.$el.find('#TimeAtAddress');

		var nCurrentValue = parseInt(jqElem.val(), 10);

		if ((nCurrentValue === 1) || (nCurrentValue === 2))
			this.$el.find(buttonContainer + ' .btn').parents('div.control-group').show();
		else {
			this.model.get('PrevPersonAddresses').reset();
			this.$el.find(buttonContainer + ' .btn').parents('div.control-group').hide();
		} // if
	}, // personalTimeAtAddressChanged

	isPrevAddressValid: function() {
		var oPersonalInfo = this.model.get('CustomerPersonalInfo');

		if (!oPersonalInfo)
			return true;

		var nTimeAtAddress = parseInt(this.$el.find('#TimeAtAddress').val(), 10);

		if ((nTimeAtAddress === 0) || (nTimeAtAddress === 3))
			return true;

		var oAddresses = this.model.get('PrevPersonAddresses');

		return oAddresses && (oAddresses.length > 0);
	}, // isPrevAddressValid

	prevModelChange: function() {
		if (this.isPrevAddressValid()) {
			this.clearAddressError('#PrevPersonAddresses');
			$('#PrevPersonAddresses .field_status').hide();
		}
		else
			$('#PrevPersonAddresses .field_status').show();

		this.inputChanged();
		EzBob.UiAction.registerView(this.prevPersonAddressesView);
	}, // prevModelChange

	render: function() {
		UnBlockUi();
		this.constructor.__super__.render.call(this);

		var oAddressContainer = this.$el.find('#PersonalAddress');
		this.personalAddressView = new EzBob.AddressView({
			model: this.model.get('PersonalAddress'),
			name: 'PersonalAddress',
			max: 1,
			uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix'),
		});
		this.personalAddressView.render().$el.appendTo(oAddressContainer);
		EzBob.Validation.addressErrorPlacement(this.personalAddressView.$el, this.personalAddressView.model);

		oAddressContainer = this.$el.find('#PrevPersonAddresses');
		this.prevPersonAddressesView = new EzBob.AddressView({
			model: this.model.get('PrevPersonAddresses'),
			name: 'PrevPersonAddresses',
			max: 3,
			uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix'),
		});
		this.prevPersonAddressesView.render().$el.appendTo(oAddressContainer);
		EzBob.Validation.addressErrorPlacement(this.prevPersonAddressesView.$el, this.prevPersonAddressesView.model);

		this.model.get('PrevPersonAddresses').on('all', this.prevModelChange, this);
		this.model.get('PersonalAddress').on('all', this.personalAddressModelChange, this);
		this.$el.find('.cashInput').moneyFormat();

		this.$el.find('.addressCaption').hide();

		var personalInfo = this.model.get('CustomerPersonalInfo');
		var predefinedPhone = undefined;
		if (personalInfo != undefined) {
			predefinedPhone = personalInfo.MobilePhone;
		} else {
			var twilioPhone = this.model.get('twilioPhone');
			if (twilioPhone != undefined) {
				predefinedPhone = twilioPhone;
			}
		}

		if (predefinedPhone) {
			var mobileObj = this.$el.find('#MobilePhone');
			mobileObj.addClass('disabled');
			mobileObj.attr("disabled", "disabled");
			mobileObj.val(predefinedPhone).change().attardi_labels('toggle');
			EzBob.Validation.element(this.validator, $(mobileObj));
		}

		var firstNameObj = this.$el.find('#FirstName');
		firstNameObj.change().attardi_labels('toggle');
		if (firstNameObj.val() != '')
			EzBob.Validation.element(this.validator, $(firstNameObj));
		var surNameObj = this.$el.find('#Surname');
		surNameObj.change().attardi_labels('toggle');
		if (surNameObj.val() != '')
			EzBob.Validation.element(this.validator, $(surNameObj));

		this.readyToProceed = true;
		return this;
	}, // render

	personalAddressModelChange: function(e, el) {
		this.inputChanged();
		this.clearAddressError('#PersonalAddress');
		EzBob.UiAction.registerView(this.personalAddressView);
	}, // personalAddressModelChange

	getValidator: function() { return EzBob.validatePersonalDetailsForm; }, // getValidator

	next: function(e) {
		var $el = $(e.currentTarget);

		if ($el.hasClass('disabled'))
			return false;

		var enabled = EzBob.Validation.checkForm(this.validator) &&
			this.isPrevAddressValid() &&
			this.isAddressValid();
	    
        if (!enabled) {
            return false;
        }
	    
		BlockUi();

		var self = this;

		var mobileObj = this.$el.find('#MobilePhone');
		if (mobileObj.hasClass('disabled')) {
			mobileObj.removeClass('disabled');
			mobileObj.attr('disabled', false);
			mobileObj.val(this.model.get('twilioPhone'));
		}

		var form = this.$el.find('form.PersonalDetailsForm');
		var data = form.serializeArray();

		if (this.$el.find('#ConsentToSearch').is(":checked"))
			_.find(data, function(d) { return d.name === 'ConsentToSearch'; }).value = true;

		var request = $.post(form.attr('action'), data);

		request.success(function(res) {
			scrollTop();

			if (res.error) {
				EzBob.App.trigger('error', res.error);
				UnBlockUi();
				return;
			} // if

			self.model.fetch().done(function() {
				self.trigger('ready');
				self.trigger('next');
			});
		});

		return false;
	}, // next
}); // EzBob.PersonalInformationStepView
