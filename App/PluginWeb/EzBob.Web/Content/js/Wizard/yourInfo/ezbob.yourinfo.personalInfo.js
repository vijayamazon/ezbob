var EzBob = EzBob || {};

EzBob.PersonalInformationStepModel = EzBob.WizardStepModel.extend({});

EzBob.PersonalInformationStepView = EzBob.YourInformationStepViewBase.extend({
	initialize: function() {
		this.template = _.template($('#personinfo-template').html());
		this.ViewName = 'personal';

		this.events = _.extend({}, this.events, {
			'change #TimeAtAddress': 'personalTimeAtAddressChanged',
            'change #ResidentialStatus': 'residentialStatusChanged',
			'change #OwnOtherProperty': 'ownOtherPropertyChanged',
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

	inputChanged: function(event) {
		var el = event ? $(event.currentTarget) : null;

		if (el && el.attr('id') === 'MiddleInitial' && el.val() === '') {
			var img = el.closest('div').find('.field_status');
			img.field_status('set', 'empty', 2);
		} // if

		var enabled = EzBob.Validation.checkForm(this.validator) &&
			this.isPrevAddressValid() &&
			this.isAddressValid() &&
			this.ownOtherPropertyIsValid();

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

	ownOtherPropertyChanged: function(evt) {
		if (this.$el.find('#OwnOtherProperty').val() === 'Yes')
			this.$el.find('#OtherPropertyAddress').parents('div.control-group').show();
		else {
			this.$el.find('#OtherPropertyAddress').parents('div.control-group').hide();
			this.model.get('OtherPropertyAddress').reset();
		} // if

		this.inputChanged();
	}, // ownOtherPropertyChanged

	ownOtherPropertyIsValid: function() {
		if (!this.model.get('IsOffline'))
			return true;

		var sValue = this.$el.find('#OwnOtherProperty').val();

		if (sValue === 'Yes') {
			var oModel = this.model.get('OtherPropertyAddress');
			return oModel && (oModel.length > 0);
		} // if

		return sValue === 'No';
	}, // ownOtherPropertyIsValid

	render: function() {
		this.constructor.__super__.render.call(this);

		this.personalAddressView = new EzBob.AddressView({ model: this.model.get('PersonalAddress'), name: 'PersonalAddress', max: 1 });
		this.personalAddressView.render().$el.appendTo(this.$el.find('#PersonalAddress'));
		this.addressErrorPlacement(this.personalAddressView.$el, this.personalAddressView.model);

		this.prevPersonAddressesView = new EzBob.AddressView({ model: this.model.get('PrevPersonAddresses'), name: 'PrevPersonAddresses', max: 3 });
		this.prevPersonAddressesView.render().$el.appendTo(this.$el.find('#PrevPersonAddresses'));
		
		this.addressErrorPlacement(this.prevPersonAddressesView.$el, this.prevPersonAddressesView.model);

		this.otherPropertyAddressView = new EzBob.AddressView({ model: this.model.get('OtherPropertyAddress'), name: 'OtherPropertyAddress', max: 1 });
		this.otherPropertyAddressView.render().$el.appendTo(this.$el.find('#OtherPropertyAddress'));
		this.addressErrorPlacement(this.otherPropertyAddressView.$el, this.otherPropertyAddressView.model);

		this.model.get('PrevPersonAddresses').on('all', this.prevModelChange, this);
		this.model.get('PersonalAddress').on('all', this.personalAddressModelChange, this);
		this.model.get('OtherPropertyAddress').on('all', this.otherPropertyAddressModelChange, this);
		this.$el.find('.cashInput').moneyFormat();

		if (!this.model.get('IsOffline'))
			this.$el.find('.offline').remove();

		this.$el.find('.addressCaption').hide();
		this.readyToProceed = true;
		return this;
	}, // render

	showConsent: function() {
		var consentAgreementModel = new EzBob.ConsentAgreementModel({
			id: this.model.get('Id'),
			firstName: this.$el.find('input[name="FirstName"]').val(),
			middleInitial: this.$el.find('input[name="MiddleInitial"]').val(),
			surname: this.$el.find('input[name="Surname"]').val()
		});

		var consentAgreement = new EzBob.ConsentAgreement({ model: consentAgreementModel });
		EzBob.App.modal.show(consentAgreement);
		return false;
	}, // showConsent

	personalAddressModelChange: function(e, el) {
		this.inputChanged();
		this.clearAddressError('#PersonalAddress');
		EzBob.UiAction.registerView(this.personalAddressView);
	}, // personalAddressModelChange

	otherPropertyAddressModelChange: function(e, el) {
		this.inputChanged();
		this.clearAddressError('#OtherPropertyAddress');
		EzBob.UiAction.registerView(this.otherPropertyAddressView);

		var oOtherProperty = this.model.get('OtherPropertyAddress');

		if (oOtherProperty && oOtherProperty.length) {
			this.$el.find('#OwnOtherProperty').val('Yes');
			this.$el.find('#OtherPropertyAddress').parents('div.control-group').show();
		}
	}, // otherPropertyAddressModelChange

	getValidator: function() { return EzBob.validatePersonalDetailsForm; }, // getValidator

	next: function(e) {
		var $el = $(e.currentTarget);

		if ($el.hasClass('disabled'))
			return false;

		BlockUi();

		var self = this;

		var form = this.$el.find('form.PersonalDetailsForm');
		var request = $.post(form.attr('action'), form.serializeArray());

		request.success(function(res) {
			scrollTop();

			if (res.error) {
				EzBob.App.trigger('error', res.error);
				return;
			} // if

			self.model.fetch().done(function() {
				self.trigger('ready');
				self.trigger('next');
			});
		});

		request.complete(function() {
			UnBlockUi();
		});

		return false;
	}, // next
}); // EzBob.PersonalInformationStepView
