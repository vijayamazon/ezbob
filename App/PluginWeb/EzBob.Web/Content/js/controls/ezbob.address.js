var EzBob = EzBob || {};

EzBob.Popup = Backbone.View.extend({
    initialize: function (options) {
		this.template = $('#address-popup-template').html();
		this.defaultPostcode = options.postcode;
		this.uiEventControlIdPrefix = options.uiEventControlIdPrefix;
		this.callback = options.callback;
		this.caller = options.caller;

		if (!window.gRootPath) {
			this.rootPath = '/';
			console.warn('window.gRootPath is not initialized!');
		}
		else
			this.rootPath = window.gRootPath;

		this.$el = $('<div class=address-dialog-widget></div>');

		EzBob.ServerLog.debug('address popup created with postcode', this.defaultPostcode);
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

		var width = 550;
		var height = 580;
		if (EzBob.Config.Origin == 'everline') {
			width = 572;
			height = 630;
		} 

		this.$el.dialog({
			autoOpen: true,
			title: 'Select address',
			modal: true,
			resizable: false,
			width: width,
			height: height,
			closeOnEscape: true,
			open: function() {
				$('body').addClass('stop-scroll');
			},
			close: function() {
				$('body').removeClass('stop-scroll');
			}
		});
		
		this.setDialogueButtons('init');

		var oWidget = this.$el.dialog('widget');

		oWidget.find('.ui-dialog-title').addClass('address-dialog-title');
		oWidget.find('.ui-dialog-titlebar').addClass('address-dialog-titlebar');
		

		this.addressList = this.$el.find('.matchingAddressList');

		this.$el.find('.postCode').val(this.defaultPostcode);

		EzBob.ServerLog.debug('address popup rendered with postcode', this.defaultPostcode);
		
		return this;
	}, // render

	setDialogueButtons: function(sMode) {
		var self = this;

		var oButtons = [];

		switch (sMode) {
		case 'init':
			oButtons.push({
				text: 'Cancel',
				'class': 'button btn-grey clean-btn',
				click: function() { self.PostCodeBtnCancel(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-cancelled-early',
			});

			break;

		case 'base':
		case 'selector':
		    oButtons.push({
		        text: 'Cancel',
		        'class': 'button btn-grey clean-btn',
		        click: function () { self.PostCodeBtnCancel(); },
		        'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-cancelled',
                sortnum:3
		    });

		    if (sMode === 'selector') {
				oButtons.push({
					text: 'Not found',
					'class': 'button btn-green ev-btn-org not-found',
					click: function() { self.PostCodeBtnNotFound(); },
					'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-not-found',
					sortnum: 2
				});
			} // if

			

			oButtons.push({
				text: 'Confirm',
				disabled: 'disabled',
				'class': 'postCodeBtnOk button btn-green disabled ev-btn-org',
				click: function() { self.PostCodeBtnOk(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-selected',
				sortnum: 1
			});

			break;

		case 'manual-input':
			oButtons.push({
				text: 'Cancel',
				'class': 'button btn-grey clean-btn',
				click: function() { self.PostCodeBtnCancel(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-manual-input-cancelled',
				sortnum: 2
			});

			oButtons.push({
				text: 'Confirm',
				'class': 'postCodeBtnManualInputOk button btn-green disabled ev-btn-org',
				disabled: 'disabled',
				click: function() { self.PostCodeBtnManualInputOk(); },
				'ui-event-control-id': this.uiEventControlIdPrefix + '-address-form:address-manual-input-selected',
				sortnum: 1
			});

			break;
		} // switch
		var sortedButtons = _.sortBy(oButtons, 'sortnum');
		this.$el.dialog('option', 'buttons', sortedButtons);

		var oWidget = this.$el.dialog('widget');

		oWidget.find('.ui-dialog-buttonpane').addClass('address-dialog-buttonpane');

		EzBob.UiAction.registerView(this);
		EzBob.ServerLog.debug('address popup buttons set to', sMode);
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

		if (EzBob.Config.Origin === 'everline') {
		    if (sVal) {
		        me.css('border-bottom', '2px solid rgb(122, 193, 67)');
		    } else {
		        if (me.hasClass('add-req')) {
		            me.css('border-bottom', '2px solid red');
		        } else {
		            me.css('border-bottom', '2px solid #c7c7c7');
		        }
		     
		    }
		} else {
		    if (sVal)
		        me.closest('label').find('img.field_status').field_status('set', 'ok');

		    else
		        me.closest('label').find('img.field_status').field_status('clear');
		}
		

		this.setManualInputOkBtnState();

		EzBob.ServerLog.debug('address popup manual form validation complete');
	}, // ValidateManualForm

	setManualInputOkBtnState: function () {
		var sLine1 = $.trim(this.$el.find('.line1').val());
		var sTown = $.trim(this.$el.find('.town').val());

		var bValid = (sLine1 !== '') && (sTown !== '');

		if (bValid)
			$('.postCodeBtnManualInputOk').removeAttr('disabled').removeClass('disabled');
		else
			$('.postCodeBtnManualInputOk').attr('disabled', 'disabled').addClass('disabled');

		EzBob.ServerLog.debug('address popup manual OK button is', bValid ? 'enabled' : 'disabled');
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
			EzBob.ServerLog.debug('address popup OK clicked but there is no current item');
			return;
		} // if

		if (id === 'NOTFOUND') {
			this.PostCodeBtnNotFound();
			EzBob.ServerLog.debug('address popup OK clicked: redirecting to manual form');
			return;
		} // if

		var addressModel;

		var oDummyResults = $('.dummy_address_search_result');

		if (oDummyResults.length > 0) {
			addressModel = $.parseJSON($('.by_id', oDummyResults.first()).html());
			addressModel.Line3 = 'Found by postcode ' + this.$el.find('.postCode').val();
		} else {
			addressModel = new EzBob.AddressModel({ Id: id });
			addressModel.fetch();
		} // if dummy

	    if (this.callback == undefined) {
	        this.model.add(addressModel);
	    } else {
	        this.callback(addressModel, this.caller);
	    }
	    
	    this.$el.dialog('close');
		this.remove();
		this.unbind();

		EzBob.ServerLog.debug('address popup OK clicked: the following address accepted:', addressModel);
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

		if (this.callback == undefined) {
	        this.model.add(addressModel);
	    } else {
	        this.callback(addressModel, this.caller);
	    }

	    this.$el.dialog('close');
		this.remove();
		this.unbind();

		EzBob.ServerLog.debug('address popup manual OK clicked: the following address accepted:', addressModel);
	}, // PostCodeBtnManualInputOk

	PostCodeBtnCancel: function () {
		this.$el.dialog('close');
		this.remove();
		this.unbind();

		EzBob.ServerLog.debug('address popup cancel clicked');
	}, // PostCodeBtnCancel

	PostCodeBtnNotFound: function() {
		var self = this;

		this.$el.find('.address-selector-block').fadeOut('fast', function() {
			self.initManualInputForm();
			EzBob.ServerLog.debug('address popup manual form shown');

		});
	}, // PostCodeBtnNotFound

	showAddressSelector: function() {
		var self = this;

		this.$el.find('.address-list-loading-block').fadeOut('fast', function() {
			self.$el.find('.address-selector-block').fadeIn('fast').removeClass('hide');
			$('.ui-dialog-buttonpane').addClass('buttons-footer');
		// if (EzBob.Config.Origin === 'everline') {#1#
			//  $('.address-dialog-widget').jScrollPane({ verticalDragMinHeight: 40 });
		//    }
		   
			EzBob.ServerLog.debug('address popup "loading" hidden, selector is shown');
		});
	}, // showAddressSelector

	initManualInputForm: function() {
	    this.$el.find('.address-input-block').fadeIn('fast').removeClass('hide');
	    $('.ui-dialog').addClass('add-scroll');
	    $('.address-dialog-widget').addClass('disable-scroll');
	    this.setDialogueButtons('manual-input');
	    $('.ui-dialog-buttonpane').removeClass('buttons-footer');
		this.$el.dialog('option', 'title', 'Enter address manually');

		this.$el.find('.zipcode').val(this.$el.find('.postCode').val().toUpperCase()).change();

		this.$el.find('img.field_status').each(function () {
			var bRequired = $(this).hasClass('required');

			var me = $(this);

			var sInitialStatus = me.closest('label').find('.form_field').val() === '' ? '' : 'ok';

			me.field_status({ required: bRequired, initial_status: sInitialStatus, });
		}); // for each field status icon

		this.$el.find('.line1').focus();
		
	    this.$el.height('auto');
		EzBob.ServerLog.debug('address popup manual form initialised');
	}, // initManualInputForm

	SearchByPostcode: function () {
		var postCode = this.$el.find('.postCode').val();
		var that = this;

		this.addressList.empty();
		this.$el.find('.postCodeBtn').attr('disabled', 'disabled');

		EzBob.ServerLog.debug('address popup started search by postcode', postCode);

		var oDoAlways = function () {
			that.showAddressSelector();

			that.addressList.trigger('liszt:updated');
			that.$el.find('.postCodeBtn').removeAttr('disabled');

			//fix for chosen select and JQuery dialog 
			$('.ui-dialog-content').css('overflow', 'visible');
			$('.ui-dialog ').css('overflow', 'visible');
			
			EzBob.ServerLog.debug('address popup completed search by postcode', postCode);
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

				EzBob.ServerLog.debug('address popup accepted the follwing entry to address list:', val);
			});
		
			that.addressList.beautifullList().removeAttr('disabled').focus();
		}; // on success

		var oDummyResults = $('.dummy_address_search_result');

		if (oDummyResults.length > 0) {
			EzBob.ServerLog.debug('address popup loading dummy results');
			var oRecords = $.parseJSON($('.by_postcode', oDummyResults.first()).html());
			oOnSuccess(oRecords);
			oDoAlways();
			return;
		} // if dummy

		var request = $.getJSON(this.rootPath + 'Postcode/GetAddressListFromPostCode', { postCode: postCode });

		request.done(function (data) {
			EzBob.ServerLog.debug('address popup GetAddressListFromPostCode(', postCode, ') finished, some data received, success is', data.Success);

			if (!data.Success || (data.Recordcount === 0)) {
				that.addressList.append($('<li></li>').val(0).html('Not found'));
				that.setDialogueButtons('base');

				EzBob.ServerLog.debug('address popup GetAddressListFromPostCode(', postCode, '): not found');

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
			EzBob.ServerLog.debug('address popup GetAddressListFromPostCode(', postCode, ') failed');

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
		this.model.on('add change remove', this.render, this);
		this.name = options.name;
		this.max = options.max || 3;
		this.title = options.title || "Enter postcode";
		this.buttonTitle = options.buttonTitle || 'Postcode lookup';
		this.isShowClear = options.isShowClear;
		this.directorId = options.directorId || 0;
		this.customerId = options.customerId || 0;
	    this.required = options.required || "required";
		this.uiEventControlIdPrefix = options.uiEventControlIdPrefix;
		this.cls = options.cls;
		this.tabindex = options.tabindex;
		EzBob.ServerLog.debug('address view initialised');
	},

	render: function () {
		var self = this;
		this.$el.html(this.template({ addresses: this.model.toJSON(), name: this.name }));

		if (this.uiEventControlIdPrefix) {
			this.$el.find('[ui-event-control-id]').each(function() {
				var oElem = $(this);

				oElem.attr('ui-event-control-id',
					self.uiEventControlIdPrefix + '-' + oElem.attr('ui-event-control-id')
				);
			});
		} // if

		this.$el.find('.label-first-line').text(this.title);
		this.$el.find('.addAddress').val(this.buttonTitle);
		this.$el.find('.btn').toggle(this.max > this.model.length);
		this.$el.find('.addAddressContainer').toggle(this.max > this.model.length);
		this.$el.find('.form_field_container.control-group').addClass(this.cls);

		

		if (this.max > this.model.length && this.model.length > 0) {
			this.$el.find('.form_field_container.control-group').removeClass('canDisabledAddress');
		}
	
		if(this.required != 'required') {
			this.$el.find('img.field_status').removeClass('required').addClass('empty');
		}

		this.postcodeInput = this.$el.find('.addAddressInput');

		if (this.tabindex) {
			this.postcodeInput.attr('tabindex', this.tabindex);
			this.$el.find('.addAddress').attr('tabindex', this.tabindex + 1);
		}

		this.showClear();

		var sInitialStatus = (this.model && this.model.length > 0 && this.required == 'required') ? 'ok' : '';
		this.$el.find('img.field_status').each(function () {
		    var bRequired = $(this).hasClass('required');
		    if (bRequired) {
		        $(this).field_status({ required: bRequired, initial_status: sInitialStatus, });
		    } else {
		        $(this).field_status('set', 'empty', 2);
		    }
		});

		_.each(this.model.models, function (val) {
			val.set({
				director: self.directorId,
				customer: self.customerId,
			}, {
				silent: true
			});
		});

		this.addAddressInputChanged();
		EzBob.ServerLog.debug('address view rendered');

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
			EzBob.ServerLog.debug('address view Clear shown');
		}
		else {
		    this.$el.find('.removeAddress').hide();
			EzBob.ServerLog.debug('address view Clear hidden');
		}
	},

	removeAddress: function (e) {
		var i = e.currentTarget.getAttribute('element-number');
		this.model.remove(this.model.at(i));
		EzBob.ServerLog.debug('address view removed address at position', i);
		return false;
	},

	addAddressInputChanged: function () {
	    var reg = /^([A-Za-z][A-Za-z0-9]?[A-Za-z0-9]?[A-Za-z0-9]?\s*[0-9][A-Za-z0-9]{2})$/;
	    if (this.postcodeInput.val().match(reg)) {
	        this.$el.find('.addAddress').removeAttr('disabled');
			EzBob.ServerLog.debug('address view .addAddress enabled');
		}
		else {
	        this.$el.find('.addAddress').attr('disabled', 'disabled');
			EzBob.ServerLog.debug('address view .addAddress disabled');
	    }
	},

	addAddress: function () {
		EzBob.ServerLog.debug('address view opening a popup');

		var popUp = new EzBob.Popup({
			model: this.model,
			postcode: $.trim(this.postcodeInput.val()),
			uiEventControlIdPrefix: this.uiEventControlIdPrefix,
		});
		popUp.render();
		popUp.SearchByPostcode();
	}
});