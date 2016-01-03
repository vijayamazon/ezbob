var EzBob = EzBob || {};

EzBob.AddDirectorInfoView = EzBob.ItemView.extend({
	template: '#add-director-info-template',

	initialize: function (options) {
		this.alreadySaved = false;

		this.backButtonCaption = options.backButtonCaption || 'Back';

		this.failOnDuplicate = options.failOnDuplicate;

		this.initDupCheck(options);

	}, // initialize

	initDupCheck: function (options) {
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

		var self = this;

		_.each(options.customerInfo.Directors, function (oDir) {
			var sKey = this.detailsToKey(
				oDir.Name,
				oDir.Surname,
				oDir.DateOfBirth,
				'DD/MM/YYYY',
				oDir.Gender,
				((oDir.DirectorAddress && oDir.DirectorAddress.length > 0) ? oDir.DirectorAddress[0].Rawpostcode : '')
			);

			self.dupCheckKeys[sKey] = 1;
		}, this);

		this.DupCheckModel = function (ary) {
			this.Name = '';
			this.Surname = '';
			this.Gender = '';
			this.DateOfBirth = '';
			this.PostCode = '';

			this.init(ary);
		};

		this.DupCheckModel.prototype = {
			init: function (ary) {
				_.each(ary, function (obj) { this.setProp(obj); }, this);
			}, // init

			readyForCheck: function () {
				return this.Name && this.Surname && this.Gender && this.DateOfBirth && this.PostCode;
			}, // readyForCheck

			setProp: function (obj) {
				if (this.hasOwnProperty(obj.name))
					this[obj.name] = obj.value;
				else if (obj.name.match(/\.Rawpostcode$/))
					this.PostCode = obj.value;
			}, // setProp

			toDetails: function () {
				return self.detailsToKey(this.Name, this.Surname, this.DateOfBirth, 'D/M/YYYY', this.Gender, this.PostCode);
			}, // toDetails
		};
	}, // initDupCheck

	form: function () {
		return this.$el.find('.addDirectorInfoForm');
	}, // form

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

		'click .is-dir-sha': 'toggleIsDirSha',
	}, // events

	toggleIsDirSha: function () {
		var oChk = $(event.target);

		var bChecked = oChk.attr('checked') ? true : false;
		var sTarget = oChk.data('target');

		this.$el.find('.' + sTarget).val(bChecked ? 'on' : 'off');

		this.canSubmit();
	}, // toggleIsDirSha

	addressModelChange: function () {
		return EzBob.App.trigger('dash-director-address-change', this.model);
	}, // addressModelChange

	setCustomerID: function (nCustomerID) {
		this.$el.find('#nCustomerID').val(nCustomerID);
	}, // setCustomerID

	onRender: function () {
		this.$el.find('.directorBack').text(this.backButtonCaption);
		this.$el.find('.ezDateTime').splittedDateTime();
		this.$el.find('.alphaOnly').alphaOnly();
		this.$el.find('.phonenumber').numericOnly(11);
		this.$el.find('.addressCaption').hide();
		this.$el.find('.addDirector').html('Add Director');
		this.$el.find('#nDirectorID').val(-1);
		this.validator = this.buildValidator();

		var oAddressContainer = this.$el.find('#DirectorAddress');

		var that = this;

		this.addressView = new EzBob.AddressView({
			model: that.model.get('DirectorAddress'),
			name: 'DirectorAddress',
			max: 1,
			uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix')
		});

		this.model.get('DirectorAddress').on('all', function () {
			that.handleUiEvent();
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

	directorBack: function () {
		this.trigger(this.backEvtName());
	}, // directorBack

	directorAdd: function () {
		if (!this.canSubmit())
			return false;

		var data = this.form().serializeArray();
		BlockUi('on');
		var self = this;
		var action = (this.$el.find('#nDirectorID').val() === '-1') ? this.form().attr('action') : '/Underwriter/CrossCheck/editDirector';
		var request = $.post(action, data);

		request.done(function (res) {
			if (res.success) {
				self.alreadySaved = true;
				self.trigger(self.successEvtName());
			}
			else {
				if (res.error)
					EzBob.App.trigger('error', res.error);
				else
					EzBob.App.trigger('error', 'Error occurred, try again');

				self.trigger(self.failEvtName());
			} // if
		}); // on success

		request.fail(function () {
			self.trigger(self.failEvtName());
		}); // on fail

		request.always(function () {
			BlockUi('off');
		}); // always

		return false;
	}, // directorAdd

	handleUiEvent: function () {
		this.canSubmit();
	}, // handleUiEvent

	isDirSha: function (sSelector) {
		return this.$el.find(sSelector).val() === 'on';
	}, // isDirSha

	canSubmit: function () {
		if (this.alreadySaved)
			return false;

		var bForm = this.validator.checkForm();
		var bHasAddress = bForm && (this.addressView.model.length > 0);
		var bHasType = bHasAddress && (this.isDirSha('.is-director') || this.isDirSha('.is-shareholder'));
		var bEnabled = bHasType && this.validateDuplicates();

		this.setSomethingEnabled(this.$el.find('.addDirector'), bEnabled);

		return bEnabled;
	}, // canSubmit

	backEvtName: function () { return 'go-back'; }, // backEvtName
	successEvtName: function () { return 'success'; }, // successEvtName
	failEvtName: function () { return 'fail'; }, // failEvtName
	dupCheckCompleteName: function () { return 'dup-check-done'; }, // dupCheckCompleteName

	setBackHandler: function (oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.backEvtName(), oHandler);
	}, // setBackHandler

	setSuccessHandler: function (oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.successEvtName(), oHandler);
	}, // setSuccessHandler

	setFailHandler: function (oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.failEvtName(), oHandler);
	}, // setFailHandler

	setDupCheckCompleteHandler: function (oHandler) {
		if (oHandler && (typeof oHandler === 'function'))
			this.on(this.dupCheckCompleteName(), oHandler);
	}, // setDupCheckCompleteHandler

	validateDuplicates: function () {
		var oModel = new this.DupCheckModel(this.form().serializeArray());

		if (!oModel.readyForCheck())
			return false;

		var bDupFound = this.dupCheckKeys.hasOwnProperty(oModel.toDetails());

		this.trigger(this.dupCheckCompleteName(), bDupFound);

		return bDupFound ? !this.failOnDuplicate : true;
	}, // validateDuplicates

	detailsToKey: function (sFirstName, sLastName, oBirthDate, sDateFormat, sGender, sPostCode) {
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

	buildValidator: function () {
		return this.form().validate({
			rules: {
				Name: EzBob.Validation.NameValidationObject,
				Surname: { required: true, maxlength: 100 },
				Gender: { required: true },
				DateOfBirth: { requiredDate: true, yearLimit: 18 },
				Email: { required: true, email: true },
				Phone: { required: true, regex: "^0[0-9]{10}$" },
			},
			messages: {
				DateOfBirth: { yearLimit: "The number of full year should be more then 18 year" },
				Phone: { regex: "Please enter a valid UK number" },
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlightFS,
			highlight: EzBob.Validation.highlightFS,
			ignore: ':not(:visible)',
		});
	}, // buildValidator
}); // EzBob.AddDirectorInfoView
