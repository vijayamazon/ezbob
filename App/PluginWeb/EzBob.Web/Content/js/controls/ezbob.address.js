var EzBob = EzBob || {};

EzBob.Popup = Backbone.View.extend({
	initialize: function (options) {
		this.template = $('#address-popup-template').html();
		this.defaultPostcode = options.postcode;
		this.uiEventControlIdPrefix = options.uiEventControlIdPrefix;

		if (!window.gRootPath) {
			this.rootPath = '/';
			console.warn('window.gRootPath is not initialized!');
		}
		else
			this.rootPath = window.gRootPath;

		this.$el = $('<div class=address-dialog-widget></div>');
	}, // initialize

	render: function () {
		var self = this;

		this.$el.html(this.template);

		if (this.uiEventControlIdPrefix) {
			this.$el.find('[ui-event-control-id]').each(function() {
				var oElem = $(this);

				oElem.attr('ui-event-control-id',
					self.uiEventControlIdPrefix + '-' + oElem.attr('ui-event-control-id')
				);
			});
		} // if

		this.$el.dialog({
			autoOpen: true,
			title: 'Select address',
			modal: true,
			resizable: true,
			width: 550,
			minWidth: 550,
			height: 580,
			minHeight: 580,
			closeOnEscape: true,
		});

		this.setDialogueButtons('init');

		$('body, .ui-widget-overlay').addClass('address-dialog-no-overflow');

		var oWidget = this.$el.dialog('widget');

		oWidget.find('.ui-dialog-title').addClass('address-dialog-title');
		oWidget.find('.ui-dialog-titlebar').addClass('address-dialog-titlebar');

		this.addressList = this.$el.find('.matchingAddressList');

		this.$el.find('.postCode').val(this.defaultPostcode);

		return this;
	}, // render

	setDialogueButtons: function(sMode) {
		var self = this;

		var oButtons = [];

		switch (sMode) {
		case 'init':
			oButtons.push({
				text: 'Cancel',
				'class': 'addr-button green',
				click: function() { self.PostCodeBtnCancel(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-cancelled-early',
			});

			break;

		case 'base':
		case 'selector':
			if (sMode === 'selector') {
				oButtons.push({
					text: 'Not found',
					'class': 'addr-button green',
					click: function() { self.PostCodeBtnNotFound(); },
					'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-not-found',
				});
			} // if

			oButtons.push({
				text: 'Cancel',
				'class': 'addr-button green',
				click: function() { self.PostCodeBtnCancel(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-cancelled',
			});

			oButtons.push({
				text: 'OK',
				disabled: 'disabled',
				'class': 'postCodeBtnOk addr-button green disabled',
				click: function() { self.PostCodeBtnOk(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-selected',
			});

			break;

		case 'manual-input':
			oButtons.push({
				text: 'Cancel',
				'class': 'addr-button green',
				click: function() { self.PostCodeBtnCancel(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-manual-input-cancelled',
			});

			oButtons.push({
				text: 'OK',
				'class': 'postCodeBtnManualInputOk addr-button green disabled',
				disabled: 'disabled',
				click: function() { self.PostCodeBtnManualInputOk(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-manual-input-selected',
			});

			break;
		} // switch

		this.$el.dialog('option', 'buttons', oButtons);

		var oWidget = this.$el.dialog('widget');

		oWidget.find('.ui-dialog-buttonpane').addClass('address-dialog-buttonpane');

		EzBob.UiAction.registerView(this);
	}, // setDialogueButtons

	events: {
		'change .postCode': 'SearchByPostcode',

		'click    .matchingAddressList': 'AddressesListClicked',
		'dblclick .matchingAddressList': 'AddressesListDoubleClicked',
		'keyup    .matchingAddressList': 'AddressesListKeyUp',
		'keydown  .matchingAddressList': 'AddressesListKeyDown',

		'change .form_field': 'ValidateManualForm',
		'paste  .form_field': 'ValidateManualForm',
		'cut    .form_field': 'ValidateManualForm',
	}, // events

	ValidateManualForm: function (evt) {
		var me = $(evt.target);

		var sVal = $.trim(me.val());

		if (sVal)
			me.closest('label').find('img.field_status').field_status('set', 'ok');
		else
			me.closest('label').find('img.field_status').field_status('clear');

		this.setManualInputOkBtnState();
	}, // ValidateManualForm

	setManualInputOkBtnState: function () {
		var sLine1 = $.trim(this.$el.find('.line1').val());
		var sTown = $.trim(this.$el.find('.town').val());

		var bValid = (sLine1 !== '') && (sTown !== '');

		if (bValid)
			$('.postCodeBtnManualInputOk').removeAttr('disabled').removeClass('disabled');
		else
			$('.postCodeBtnManualInputOk').attr('disabled', 'disabled').addClass('disabled');
	}, // setManualButtoInputState

	AddressesListKeyDown: function(evt) {
		switch (evt.which) {
			case 13: // Enter
			case 27: // Cancel
			case 37: // Left
			case 72: // h
			case 38: // Up
			case 75: // k
			case 40: // Down
			case 74: // j
			case 39: // Right
			case 76: // l
			case 33: // Page Up
			case 34: // Page Down
			case 32: // Space
			case 36: // Home
			case 35: // End
				evt.preventDefault();
				break;
		} // switch
	}, // AddressesListKeyDown

	AddressesListKeyUp: function (evt) {
		var oSelected = this.addressList.find('li[selected]');

		var oNewItem = null;
		var oLst = null;

		switch (evt.which) {
			case 13: // Enter
				this.PostCodeBtnOk();
				break;

			case 27: // Cancel
				this.PostCodeBtnCancel();
				break;

			case 37: // Left
			case 72: // h
			case 38: // Up
			case 75: // k
				if (oSelected.length === 1)
					oNewItem = oSelected.prev();
				break;

			case 40: // Down
			case 74: // j
			case 39: // Right
			case 76: // l
				if (oSelected.length === 1)
					oNewItem = oSelected.next();
				break;

			case 33: // Page Up
				if (oSelected.length === 1)
					oLst = oSelected.prevAll();
				break;

			case 34: // Page Down
			case 32: // Space
				if (oSelected.length === 1)
					oLst = oSelected.nextAll();
				break;

			case 36: // Home
				oNewItem = this.addressList.children().first();
				break;

			case 35: // End
				oNewItem = this.addressList.children().last();
				break;
		} // switch

		if (oLst)
			if (oLst.length >= 5)
				oNewItem = oLst.eq(5);

		if (oNewItem) {
			oNewItem.click();
			this.$el.scrollTo(oNewItem);
		} // if
	}, // AddressesListKeyUp

	AddressesListClicked: function (evt) {
		EzBob.UiAction.saveOne(EzBob.UiAction.evtClick(), evt.target);
		$('.postCodeBtnOk').removeAttr('disabled').removeClass('disabled');

		if ($(evt.target).hasClass('not-found-item'))
			this.PostCodeBtnOk();
	}, // AddressesListClicked

	AddressesListDoubleClicked: function (evt) {
		this.AddressesListClicked(evt);
		EzBob.UiAction.saveOne(EzBob.UiAction.evtLinked(), evt.target);
		this.PostCodeBtnOk();
	}, // AddressesListDoubleClicked

	PostCodeBtnOk: function () {
		var id = this.addressList.attr('data');

		if (!id || (id === '0')) {
			this.addressList.css('border', '1px solid red');
			return;
		} // if

		if (id === 'NOTFOUND') {
			this.PostCodeBtnNotFound();
			return;
		} // if

		var addressModel;

		var oDummyResults = $('.dummy_address_search_result');

		if (oDummyResults.length > 0) {
			addressModel = $.parseJSON($('.by_id', oDummyResults.first()).html());
			addressModel.Line3 = 'Found by postcode ' + this.$el.find('.postCode').val();
		} else {
			addressModel = new EzBob.AddressModel({ id: id });
			addressModel.fetch();
		} // if dummy

		this.model.add(addressModel);

		this.$el.dialog('close');
		this.remove();
		this.unbind();
		$('body').removeClass('address-dialog-no-overflow');
	}, // PostCodeBtnOk

	PostCodeBtnManualInputOk: function () {
		var dNow = new Date();

		var addressModel = {
			"found": "1",
			"Credits_display_text": "",
			"Accountadminpage": "",
			"errormessage": "",
			"AddressId": "MANUAL_" + dNow,
			"Id": "MANUAL_" + dNow,
			"Organisation": "",
			"Line1": this.$el.find('.line1').val(),
			"Line2": this.$el.find('.line2').val(),
			"Line3": this.$el.find('.line3').val(),
			"Town": this.$el.find('.town').val(),
			"County": "",
			"Postcode": this.$el.find('.zipcode').val(),
			"Country": this.$el.find('.country').val(),
			"Rawpostcode": this.$el.find('.zipcode').val(), 
			"Deliverypointsuffix": "1P",
			"Nohouseholds": "1",
			"Smallorg": "N",
			"Pobox": this.$el.find('.pobox').val(),
			"Mailsortcode": "0",
			"Udprn": "0"
		};

		this.model.add(addressModel);

		this.$el.dialog('close');
		this.remove();
		this.unbind();
		$('body').removeClass('address-dialog-no-overflow');
	}, // PostCodeBtnManualInputOk

	PostCodeBtnCancel: function () {
		this.$el.dialog('close');
		this.remove();
		this.unbind();
		$('body').removeClass('address-dialog-no-overflow');
	}, // PostCodeBtnCancel

	PostCodeBtnNotFound: function() {
		var self = this;

		this.$el.find('.address-selector-block').fadeOut('slow', function() { self.initManualInputForm(); });
	}, // PostCodeBtnNotFound

	showAddressSelector: function() {
		var self = this;

		this.$el.find('.address-list-loading-block').fadeOut('slow', function() {
			self.$el.find('.address-selector-block').fadeIn('slow').removeClass('hide');
		});
	}, // showAddressSelector

	initManualInputForm: function() {
		this.$el.find('.address-input-block').fadeIn('slow').removeClass('hide');
		this.setDialogueButtons('manual-input');
		this.$el.dialog('option', 'title', 'Enter address manually');

		this.$el.find('.zipcode').val(this.$el.find('.postCode').val().toUpperCase()).change();

		this.$el.find('img.field_status').each(function () {
			var bRequired = $(this).hasClass('required');

			var me = $(this);

			var sInitialStatus = me.closest('label').find('.form_field').val() === '' ? '' : 'ok';

			me.field_status({ required: bRequired, initial_status: sInitialStatus, });
		}); // for each field status icon

		this.$el.find('.line1').focus();
	}, // initManualInputForm

	SearchByPostcode: function () {
		var postCode = this.$el.find('.postCode').val();
		var that = this;

		this.addressList.empty();
		this.$el.find('.postCodeBtn').attr('disabled', 'disabled');

		var oDoAlways = function () {
			that.showAddressSelector();

			that.addressList.trigger('liszt:updated');
			that.$el.find('.postCodeBtn').removeAttr('disabled');

			//fix for chosen select and JQuery dialog 
			$('.ui-dialog-content').css('overflow', 'visible');
			$('.ui-dialog ').css('overflow', 'visible');
		}; // do always

		var sAddressEntryUiEventControlID = 'address-form:address-entry';

		if (this.uiEventControlIdPrefix)
			sAddressEntryUiEventControlID = this.uiEventControlIdPrefix + '-' + sAddressEntryUiEventControlID;

		var oOnSuccess = function (oRecordList) {
			$.each(oRecordList, function (i, val) {
				that.addressList.append(
					$('<li></li>')
						.addClass(val.Id === 'NOTFOUND' ? 'not-found-item' : '')
						.attr({
							data: val.Id,
							'ui-event-control-id': sAddressEntryUiEventControlID,
						})
						.html(val.L)
				);
			});

			that.addressList.beautifullList().removeAttr('disabled').focus();
		}; // on success

		var oDummyResults = $('.dummy_address_search_result');

		if (oDummyResults.length > 0) {
			var oRecords = $.parseJSON($('.by_postcode', oDummyResults.first()).html());
			oOnSuccess(oRecords);
			oDoAlways();
			return;
		} // if dummy

		var request = $.getJSON(this.rootPath + 'Postcode/GetAddressListFromPostCode', { postCode: postCode });

		request.done(function (data) {
			if (!data.Success || (data.Recordcount === 0)) {
				that.addressList.append($('<li></li>').val(0).html('Not found'));
				that.setDialogueButtons('base');

				return;
			} // if

			data.Records.push({
				Id: 'NOTFOUND',
				L: 'Address is not listed',
			});

			that.setDialogueButtons('selector');

			oOnSuccess(data.Records);
		}); // on success

		request.fail(function () {
			that.addressList.append($('<li></li>').val(0).html('Not found'));
			that.setDialogueButtons('base');

			that.addressList.attr('disabled', 'disabled');
			that.$el.find('.postCodeBtnOk').attr('disabled', 'disabled').addClass('disabled');
		}); // on fail

		request.always(oDoAlways);
	}, // SearchByPostcode
}); // EzBob.Popup

EzBob.AddressView = Backbone.View.extend({
	initialize: function (options) {
		this.template = _.template($('#address-template').html());
		this.model.on('all', this.render, this);
		this.name = options.name;
		this.max = options.max || 5;
	    this.title = options.title || "Enter postcode";
		this.isShowClear = options.isShowClear;
		this.directorId = options.directorId || 0;
		this.customerId = options.customerId || 0;
		this.uiEventControlIdPrefix = options.uiEventControlIdPrefix;
	},

	render: function () {
		var self = this;

		this.$el.html(this.template({ addresses: this.model.toJSON(), name: this.name, title: this.title }));

		if (this.uiEventControlIdPrefix) {
			this.$el.find('[ui-event-control-id]').each(function() {
				var oElem = $(this);

				oElem.attr('ui-event-control-id',
					self.uiEventControlIdPrefix + '-' + oElem.attr('ui-event-control-id')
				);
			});
		} // if

		this.$el.find('.btn').toggle(this.max > this.model.length);
		this.$el.find('.addAddressContainer').toggle(this.max > this.model.length);
		this.postcodeInput = this.$el.find('.addAddressInput');
		this.showClear();

		var sInitialStatus = (this.model && this.model.length) ? 'ok' : '';

		this.$el.find('img.field_status').each(function () {
			var bRequired = $(this).hasClass('required');
			$(this).field_status({ required: bRequired, initial_status: sInitialStatus, });
		});

		_.each(this.model.models, function (val) {
			val.set({
				director: self.directorId,
				customer: self.customerId,
			}, {
				silent: true
			});
		});

		return this;
	},

	events: {
		'click .removeAddress': 'removeAddress',
		'click .addAddress': 'addAddress',
		'keyup .addAddressInput': 'addAddressInputChanged',
		'change .addAddressInput': 'addAddressInputChanged'
	},

	showClear: function () {
		if (this.isShowClear || (this.isShowClear === undefined)) {
			this.$el.find('.removeAddress').show();
		} else {
			this.$el.find('.removeAddress').hide();
		}
	},

	removeAddress: function (e) {
		var i = e.currentTarget.getAttribute('element-number');
		this.model.remove(this.model.at(i));
		return false;
	},

	addAddressInputChanged: function () {
		if (this.postcodeInput.val().length > 0)
			this.$el.find('.addAddress').removeAttr('disabled');
		else
			this.$el.find('.addAddress').attr('disabled', 'disabled');
	},

	addAddress: function () {
		var popUp = new EzBob.Popup({
			model: this.model,
			postcode: $.trim(this.postcodeInput.val()),
			uiEventControlIdPrefix: this.uiEventControlIdPrefix,
		});
		popUp.render();
		popUp.SearchByPostcode();
	}
});