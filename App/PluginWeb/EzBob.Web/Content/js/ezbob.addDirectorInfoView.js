﻿var EzBob = EzBob || {};

EzBob.AddDirectorInfoView = EzBob.ItemView.extend({
	template: '#add-director-info-template',

	initialize: function(options) {
		this.backButtonCaption = options.backButtonCaption || 'Back';

		this.failOnDuplicate = options.failOnDuplicate;

		this.initDupCheck(options);
	}, // initialize

	initDupCheck: function(options) {
		this.dupCheckKeys = {};

		var sKey = this.detailsToKey(
			options.customerInfo.FirstName,
			options.customerInfo.Surname,
			options.customerInfo.DateOfBirth,
			null,
			options.customerInfo.Gender,
			options.customerInfo.PostCode
		);

		this.dupCheckKeys[sKey] = 1;

		_.each(options.customerInfo.Directors, function (oDir) {
		    if (oDir.IsExperianDirector || oDir.IsExperianShareholder) {
		        return;
		    }
			var sKey = this.detailsToKey(
				oDir.Name,
				oDir.Surname,
				oDir.DateOfBirth,
				'DD/MM/YYYY',
				oDir.Gender,
				oDir.DirectorAddress[0].Rawpostcode
			);

			this.dupCheckKeys[sKey] = 1;
		}, this);

		var self = this;

		this.DupCheckModel = function(ary) {
			this.Name = '';
			this.Surname = '';
			this.Gender = '';
			this.DateOfBirth = '';
			this.PostCode = '';

			this.init(ary);
		};

		this.DupCheckModel.prototype = {
			init: function(ary) {
				_.each(ary, function(obj) { this.setProp(obj); }, this);
			}, // init

			readyForCheck: function() {
				return this.Name && this.Surname && this.Gender && this.DateOfBirth && this.PostCode;
			}, // readyForCheck

			setProp: function(obj) {
				if (this.hasOwnProperty(obj.name))
					this[obj.name] = obj.value;
				else if (obj.name.match(/\.Rawpostcode$/))
					this.PostCode = obj.value;
			}, // setProp

			toDetails: function() {
				return self.detailsToKey(this.Name, this.Surname, this.DateOfBirth, 'D/M/YYYY', this.Gender, this.PostCode);
			}, // toDetails
		};
	}, // initDupCheck

	region: {
		directorAddress: '.director_address'
	}, // region

	ui: {
		form: '.addDirectorInfoForm',
		addButton: '.addDirector'
	}, // ui

	events: {
		'click .directorBack': 'directorBack',
		'click .addDirector': 'directorAdd',

		'change   input': 'handleUiEvent',
		'click    input': 'handleUiEvent',
		'focusout input': 'handleUiEvent',
		'keyup    input': 'handleUiEvent',

		'change   select': 'handleUiEvent',
		'click    select': 'handleUiEvent',
		'focusout select': 'handleUiEvent',
		'keyup    select': 'handleUiEvent',
	}, // events

	addressModelChange: function() {
		return EzBob.App.trigger('dash-director-address-change', this.model);
	}, // addressModelChange

	setCustomerID: function(nCustomerID) {
		this.$el.find('#nCustomerID').val(nCustomerID);
	}, // setCustomerID

	onRender: function() {
		this.$el.find('.directorBack').text(this.backButtonCaption);
		this.$el.find('.ezDateTime').splittedDateTime();
		this.$el.find('.alphaOnly').alphaOnly();
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.addressCaption').hide();

		this.validator = EzBob.validateAddDirectorForm(this.ui.form);

		var oAddressContainer = this.$el.find('#DirectorAddress');

		var that = this;

		this.addressView = new EzBob.AddressView({
			model: that.model.get('DirectorAddress'),
			name: 'DirectorAddress',
			max: 1,
			uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix')
		});

		this.model.get('DirectorAddress').on('all', function() {
			return that.trigger('director:addressChanged');
		});

		this.addressView.render().$el.appendTo(oAddressContainer);

		EzBob.Validation.addressErrorPlacement(this.addressView.$el, this.addressView.model);

		EzBob.UiAction.registerView(this);

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.canSubmit();

		return this;
	}, // onRender

	directorBack: function() {
		this.trigger(this.backEvtName());
	}, // directorBack

	directorAdd: function() {
		if (!this.canSubmit())
			return false;

		var data = this.ui.form.serializeArray();

		BlockUi('on');

		var request = $.post(this.ui.form.attr('action'), data);

		var self = this;

		request.done(function(res) {
			if (res.success)
				self.trigger(self.successEvtName());
			else {
				if (res.error)
					EzBob.App.trigger('error', res.error);
				else
					EzBob.App.trigger('error', 'Error occurred, try again');

				self.trigger(self.failEvtName());
			} // if
		}); // on success

		request.fail(function() {
			self.trigger(self.failEvtName());
		}); // on fail

		request.always(function() {
			BlockUi('off');
		}); // always

		return false;
	}, // directorAdd

	handleUiEvent: function() {
		this.canSubmit();
	}, // handleUiEvent

	canSubmit: function() {
		var bEnabled = this.validator.checkForm() &&
			(this.addressView.model.length > 0) &&
			this.validateDuplicates();

		this.setSomethingEnabled(this.ui.addButton, bEnabled);

		return bEnabled;
	}, // canSubmit

	backEvtName: function() { return 'go-back'; }, // backEvtName
	successEvtName: function() { return 'success'; }, // successEvtName
	failEvtName: function() { return 'fail'; }, // failEvtName
	dupCheckCompleteName: function() { return 'dup-check-done'; }, // dupCheckCompleteName

	setBackHandler: function(oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.backEvtName(), oHandler);
	}, // setBackHandler

	setSuccessHandler: function(oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.successEvtName(), oHandler);
	}, // setSuccessHandler

	setFailHandler: function(oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.failEvtName(), oHandler);
	}, // setFailHandler

	setDupCheckCompleteHandler: function(oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.dupCheckCompleteName(), oHandler);
	}, // setDupCheckCompleteHandler

	validateDuplicates: function() {
		var oModel = new this.DupCheckModel(this.ui.form.serializeArray());

		if (!oModel.readyForCheck())
			return false;

		var bDupFound = this.dupCheckKeys.hasOwnProperty(oModel.toDetails());

		this.trigger(this.dupCheckCompleteName(), bDupFound);

		return bDupFound ? !this.failOnDuplicate : true;
	}, // validateDuplicates

	detailsToKey: function(sFirstName, sLastName, oBirthDate, sDateFormat, sGender, sPostCode) {
		var oDate = sDateFormat ? moment(oBirthDate, sDateFormat) : moment(oBirthDate);
		var sBirthDate = '';

		if (oDate)
			sBirthDate = oDate.utc().format('YYYY-MM-DD');

		switch (sGender) {
			case 0:
			case '0':
			case 'm':
			case 'M':
				sGender = 'M';
				break;

			case 1:
			case '1':
			case 'f':
			case 'F':
				sGender = 'F';
				break;
		} // switch

		return JSON.stringify({ f: sFirstName, l: sLastName, b: sBirthDate, g: sGender, p: sPostCode, });
	}, // detailsToKey
}); // EzBob.AddDirectorInfoView
