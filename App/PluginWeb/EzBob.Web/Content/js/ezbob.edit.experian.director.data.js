var EzBob = EzBob || {};

(function() {
	EzBob.EditExperianDirectorData = function(options) {
		this.init(options);
	}; // EzBob.EditExperianDirectorData

	EzBob.EditExperianDirectorData.prototype.init = function(options) {
		var defaults = {
			directorID: 0,
			email: '',
			mobilePhone: '',
			line1: '',
			line2: '',
			line3: '',
			town: '',
			county: '',
			postcode: '',
		};

		_.extend(this, defaults, options);
	}; // EzBob.EditExperianDirectorData.init

	// end of EzBob.EditExperianDirectorData

	EzBob.EditExperianDirectorView = function(options) {
		this.init(options);
	}; // EzBob.EditExperianDirectorView

	_.extend(EzBob.EditExperianDirectorView.prototype, EzBob.SimpleView.prototype);

	EzBob.EditExperianDirectorView.prototype.init = function(options) {
		var defaults = {
			data: null, // EzBob.EditExperianDirectorData

			saveUrl: '',

			editBtn: null,
			saveBtn: null,
			cancelBtn: null,

			row: null,
			emailCell: null,
			mobilePhoneCell: null,
			addressCell: null,

			additionalData: null,
		};

		_.extend(this, defaults, options);
	}; // EzBob.EditExperianDirectorView.init

	EzBob.EditExperianDirectorView.prototype.render = function() {
		var oTemplate = $('#edit-experian-director-data-template');

		this.emailCell.addClass('narrow-as-possible edit-mode').empty().append(oTemplate.find('.email-container').clone(true));

		this.mobilePhoneCell.addClass('narrow-as-possible edit-mode').empty().append(oTemplate.find('.phone-container').clone(true));
		this.mobilePhoneCell.find('.mobilePhone').numericOnly(this.phoneNumberLength);

		this.addressCell.addClass('narrow-as-possible edit-mode').empty().append(oTemplate.find('.address-container').clone(true));

		this.row.find('img.field_status').each(function() { $(this).css({ display: '', visibility: '', }); });

		var oIcons = this.row.find('img.field_status');
		oIcons.filter('.required').field_status({ required: true, });
		oIcons.not('.required').field_status({ required: false, });

		var oHandler = _.bind(this.inputChanged, this);

		for (var sField in this.data) {
			if (sField === 'directorID')
				continue;

			this.row.find('.' + sField)
				.on('click keyup keydown change cut paste', oHandler)
				.val(this.data[sField]).change();
		} // for

		this.editBtn.addClass('hide').hide();
		this.saveBtn.removeClass('hide').show().click(_.bind(this.save, this));
		this.cancelBtn.removeClass('hide').show().click(_.bind(this.cancel, this));

		this.validateAll(null, true);

		this.emailCell.find('.email').focus();
	}; // EzBob.EditExperianDirectorView.prototype.render

	EzBob.EditExperianDirectorView.prototype.save = function() {
		if (!this.validateAll())
			return;

		BlockUi();

		for (var sField in this.data)
			if (sField !== 'directorID')
				this.data[sField] = $.trim(this.row.find('.' + sField).val());

		var oRequest = $.post(this.saveUrl, _.extend({}, this.additionalData, this.data));

		var self = this;

		oRequest.done(function(oResponse) {
			if (oResponse.success) {
				EzBob.App.trigger('clear');
				self.cancel();
				return;
			} // if

			if (oResponse.error)
				EzBob.App.trigger('error', oResponse.error);
			else
				EzBob.App.trigger('error', 'Error saving director data.');
		});

		oRequest.fail(function() {
			EzBob.App.trigger('error', 'Failed to save director data.');
		});

		oRequest.always(function() {
			UnBlockUi();
		});
	}; // EzBob.EditExperianDirectorView.prototype.save

	EzBob.EditExperianDirectorView.prototype.cancel = function() {
		this.emailCell.removeClass('narrow-as-possible edit-mode').empty().text(this.data.email);
		this.mobilePhoneCell.removeClass('narrow-as-possible edit-mode').empty().text(this.data.mobilePhone);
		this.addressCell.removeClass('narrow-as-possible edit-mode').empty().text(
			_.filter([this.data.line1, this.data.line2, this.data.line3, this.data.town, this.data.county, this.data.postcode], $.trim).join(' ')
		);

		this.editBtn.removeClass('hide').show();
		this.saveBtn.addClass('hide').hide().off();
		this.cancelBtn.addClass('hide').hide().off();
	}; // EzBob.EditExperianDirectorView.prototype.cancel

	EzBob.EditExperianDirectorView.prototype.inputChanged = function() {
		this.validateAll($(event.target));
	}; // EzBob.EditExperianDirectorView.prototype.inputChanged

	EzBob.EditExperianDirectorView.prototype.validateAll = function(oTarget, bAnimate) {
		var bIsFormValid = oTarget ? this.validateOne(oTarget, true) : true;

		for (var sField in this.data) {
			if (sField === 'directorID')
				continue;

			var oField = this.row.find('.' + sField);

			if (oTarget && (oField === oTarget))
				continue;

			var bIsThisValid = this.validateOne(oField, bAnimate);

			bIsFormValid = bIsFormValid && bIsThisValid;
		} // for

		this.setSomethingEnabled(this.saveBtn, bIsFormValid);

		return bIsFormValid;
	}; // EzBob.EditExperianDirectorView.prototype.validateAll

	EzBob.EditExperianDirectorView.prototype.validateOne = function(oInputEl, bAnimate) {
		var oInput = $(oInputEl);
		var sVal = $.trim(oInput.val());
		var oIcon = oInput.closest('label').find('.field_status');

		if (oInput.hasClass('email')) {
			var reEmail = /^[A-Z0-9._%+-]+@[A-Z0-9.-_]+\.[A-Z]{2,4}$/i;

			if (reEmail.test(sVal)) {
				if (bAnimate)
					oIcon.field_status('set', 'ok');

				return true;
			} // if

			if (bAnimate) {
				if (sVal)
					oIcon.field_status('set', 'fail', 2);
				else
					oIcon.field_status('clear');
			} // if

			return false;
		} // if email

		if (oInput.hasClass('mobilePhone')) {
			if (sVal && (sVal.length === this.phoneNumberLength) && (sVal[0] === '0')) {
				if (bAnimate)
					oIcon.field_status('set', 'ok');

				return true;
			} // if

			if (bAnimate) {
				if (sVal)
					oIcon.field_status('set', 'fail', 2);
				else
					oIcon.field_status('clear');
			} // if

			return false;
		} // if phone

		if (sVal) {
			if (bAnimate)
				oIcon.field_status('set', 'ok');

			return true;
		}
		else {
			if (bAnimate)
				oIcon.field_status('clear');

			return !oIcon.hasClass('required');
		} // if
	}; // EzBob.EditExperianDirectorView.prototype.validateOne

	EzBob.EditExperianDirectorView.prototype.phoneNumberLength = 11;

	// end of EzBob.EditExperianDirectorView
})();
