var EzBob = EzBob || {};

EzBob.EditLoanView = Backbone.Marionette.ItemView.extend({
	template: '#loan_editor_template',

	scheduleTemplate: $('#loan_editor_schedule_template').length > 0 ? _.template($('#loan_editor_schedule_template').html()) : null,

	initialize: function() {
		this.bindTo(this.model, 'change sync', this.render, this);
		this.modelBinder = new Backbone.ModelBinder();
		this.editItemIndex = -1;
	}, // initialize
	bindings: {
	},//bindings
	serializeData: function() {
		var data = this.model.toJSON();
		data.editItemIndex = this.editItemIndex;
		data.hasFreezInterest = this.hasFreeIntrest();
		return data;
	}, // serializeData

	hasFreeIntrest: function() {
		var freezArray = this.model.get('SInterestFreeze');

		if (freezArray === null)
			return false;
		var flag = false;
		for (var i = 0; i < freezArray.length; i++) {
			var item = freezArray[i].split('|');
			if (item[4] === '') {
				flag = true;
				break;
			}
		}
		return flag;
	},//hasFreeIntrest

	ui: {
		scheduleEl: '.editloan-schedule-region',
		freezeEl: '.editloan-freeze-intervals-region',
		ok: '.save',
		buttons: '.buttons',
		err_head: '#err-head',
		err_body: '#err-body',
		err_footer: '#err-footer',
		err_region: '#err-region'
	}, // ui

	editors: {
		'Installment': EzBob.InstallmentEditor,
		'Fee': EzBob.FeeEditor,
	}, // editors

	events: {
		'click #resch-submit-btn': 'reschSubmitForm',
		'blur #outsidePrincipal': 'onChangeAmount',
		'change #withinSelect': 'withinSelectChange',
		'change #outsideSelect': 'outsideSelectChange',
		'click .edit-schedule-item': 'editScheduleItem',
		'click .remove-schedule-item': 'removeScheduleItem',
		//   'click .add-installment': 'addInstallment',
		'click .add-fee': 'addFee',
		'click .save': 'onOk',
		'click .cancel': 'onCancel',
		'click .add-freeze-interval': 'onAddFreezeInterval',
		'click .remove-freeze-interval': 'onRemoveFreezeInterval'
	}, // events

	fillErrorPopup: function(params) {
		var self = this;
		this.ui.err_head.text(params.head);
		this.ui.err_body.text(params.body);
		this.ui.err_footer.text(params.footer);
		this.ui.err_head.css('color', params.color);
		this.ui.err_region.css('border-color', params.color);
		this.ui.err_region.fadeIn();
		for (var i = 0; i < params.selectors.length; i++)
			params.selectors[i].addClass('err-field-red');
		setTimeout(function() {
			for (var i = 0; i < params.selectors.length; i++)
				params.selectors[i].removeClass('err-field-red');
			self.ui.err_region.fadeOut();
		}, params.timeout);
	},

	withinSelectChange: function() {
		var duration = $('#withinSelect option:selected').text();
		var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: duration, rescheduleIn: 'true', save: 'false' });
		var self = this;
		BlockUi('on');
		request.success(function(res) {
			$('#withinPayments').text(res.IntervalsNum);
			$('#withinPrincipal').text(EzBob.formatPounds(res.ReschedulingBalance));
			$('#withinIntrest').text(EzBob.formatPounds(res.FirstPaymentInterest));
		}); //on success

		request.fail(function() {
			var params = { head: 'Data transmission failed', body: 'if this error returns, please contact support', footer: 'Please try sending again', color: 'red', selectors: [], timeout: '7000' };
			self.fillErrorPopup(params);
		});//on fail

		request.always(function() {
			BlockUi('off');
		});
	},
	outsideSelectChange: function() {
		this.ui.err_region.fadeOut();
		$('#outsidePrincipal').removeClass('err-field-red');
		var amount = $("#outsidePrincipal").val();
		var duration = $('#outsideSelect option:selected').text();
		if (typeof amount === 'undefined' || amount <= 0) {
			var params = { head: 'Please fix the marked field', body: 'Only positive numeric values allowed', footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#outsidePrincipal')], timeout: '60000' };
			this.fillErrorPopup(params);
		} else {
			var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: duration, AmountPerInterval: amount, rescheduleIn: 'false', save: 'false' });
			var self = this;
			BlockUi('on');
			request.success(function(res) {
				$('#outsidePayments').text(res.IntervalsNum);
				$('#outsideIntrest').text(EzBob.formatPounds(res.FirstPaymentInterest));
				if (res.Error != null && res.Error.length > 0) {
					var params = { head: 'Please fix the marked field', body: res.Error, footer: 'Please update and click Submit to continue', color: 'red', selectors: [self.$el.find('#outsidePrincipal')], timeout: '60000' };
					self.fillErrorPopup(params);
				}
			}); //on success

			request.fail(function() {
				var params = { head: 'Data transmission failed', body: 'if this error returns, please contact support', footer: 'Please try sending again', color: 'red', selectors: [], timeout: '7000' };
				self.fillErrorPopup(params);
			});//on fail

			request.always(function() {
				BlockUi('off');
			});
		}
	},
	reschSubmitForm: function() {

		var requestParam = { loanID: this.model.get('Id'), save: 'true' };
		var submitBlank = true;
		var checkedRadio = $('input[name=rescheduleIn]').filter(':checked').val();
		if (checkedRadio === 'true') {
			requestParam.intervalType = $('#withinSelect option:selected').text();
			requestParam.rescheduleIn = 'true';
			submitBlank = false;
		}
		if (checkedRadio === 'false') {
			requestParam.intervalType = $('#outsideSelect option:selected').text();
			requestParam.AmountPerInterval = $('#outsidePrincipal').val();
			requestParam.rescheduleIn = 'false';
			submitBlank = false;
		}

		var isStopCharges = $('#automatic-charges').is(':checked');
		var isStopIntrest = $("#intrest-checkbox").is(':checked');
		var isStopFees = $('#fees-calculation').is(':checked');

		var chargesVal = $('#charges-payments').val();
		var interestFrom = EzBob.formatDateTimeCS($('#intrest-calendar-from').val());
		var interestTo = EzBob.formatDateTimeCS($('#intrest-calendar-to').val());
		var feesFrom = EzBob.formatDateTimeCS($('#fees-calendar-from').val());
		var feesTo = EzBob.formatDateTimeCS($('#fees-calendar-to').val());


		if (isStopCharges) {
			if (chargesVal < 0 || chargesVal === "") {
				var params = { head: 'Please fix the marked field', body: 'Number of payments must be 0 or greater', footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#charges-payments')], timeout: '7000' };
				this.fillErrorPopup(params);
				return false;
			}
			requestParam.stopAutoCharge = 'true';
			requestParam.stopAutoChargePayment = chargesVal;
			submitBlank = false;
		}
		if (isStopFees) {
			if (feesTo === "" || feesFrom === "") {
				var params = { head: 'Please fix the marked fields', body: 'Both dates must be set for this operation', footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#fees-calendar-from'), this.$el.find('#fees-calendar-to')], timeout: '7000' };
				this.fillErrorPopup(params);
				return false;
			}
			if (new Date(feesFrom).getTime() > new Date(feesTo).getTime()) {
				var params = { head: 'Please fix the marked fields', body: 'Until date must be greater then From date', footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#fees-calendar-from'), this.$el.find('#fees-calendar-to')], timeout: '7000' };
				this.fillErrorPopup(params);
				return false;
			}
			requestParam.stopLateFee = 'true';
			requestParam.lateFeeStartDate = feesFrom;
			requestParam.lateFeeEndDate = feesTo;
			submitBlank = false;
		}
		if (isStopIntrest) {
			if (interestTo === "" || interestFrom === "") {
				var params = { head: 'Please fix the marked fields', body: 'Both dates must be set for this operation', footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#intrest-calendar-from'), this.$el.find('#intrest-calendar-to')], timeout: '7000' };
				this.fillErrorPopup(params);
				return false;
			}
			if (new Date(interestFrom).getTime() > new Date(interestTo).getTime()) {
				var params = { head: 'Please fix the marked fields', body: 'Until date must be greater then From date', footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#intrest-calendar-from'), this.$el.find('#intrest-calendar-to')], timeout: '7000' };
				this.fillErrorPopup(params);
				return false;
			}
			requestParam.freezeInterest = 'true';
			requestParam.freezeStartDate = interestFrom;
			requestParam.freezeEndDate = interestTo;
			submitBlank = false;
		}
		if (submitBlank) {
			var params = { head: 'No actions were selected', body: '', footer: 'Please make the desired changes and click submit', color: 'red', selectors: [], timeout: '7000' };
			this.fillErrorPopup(params);
			return false;
		}

		var oRequest = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', requestParam);

		var self = this;
		BlockUi('on');

		oRequest.success(function(res) {
			if (res.Error == null || res.Error === "") {
				var params = { head: 'Data has been successfuly sent to server', body: '', footer: 'Window will auto close', color: 'green', selectors: [], timeout: '7000' };
				self.fillErrorPopup(params);

				setTimeout(function() { self.close(); }, 3500);
			} else {
				var params = { head: 'Unexpected error occured', body: res.Error, footer: 'Please try sending again', color: 'red', selectors: [], timeout: '7000' };
				self.fillErrorPopup(params);
			}
			return false;
		}); //on success

		oRequest.fail(function() {
			var params = { head: 'Data transmission failed', body: 'if this error returns, please contact support', footer: 'Please try sending again', color: 'red', selectors: [], timeout: '7000' };
			self.fillErrorPopup(params);
			return false;
		});//on fail
		oRequest.always(function() {
			BlockUi('off');
		});
	},

	onChangeAmount: function() {
		this.ui.err_region.fadeOut();
		$('#outsidePrincipal').removeClass('err-field-red');
		var amount = $("#outsidePrincipal").val();
		var duration = $('#outsideSelect option:selected').text();
		if (typeof amount === 'undefined' || amount <= 0) {
			var params = { head: 'Please fix the marked field', body: 'Only positive numeric values allowed', footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#outsidePrincipal')], timeout: '60000' };
			this.fillErrorPopup(params);
		} else {
			var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: duration, AmountPerInterval: amount, rescheduleIn: 'false', save: 'false' });
			var self = this;
			BlockUi('on');
			request.success(function(res) {
				$('#outsidePayments').text(res.IntervalsNum);
				$('#outsideIntrest').text(EzBob.formatPounds(res.FirstPaymentInterest));
				if (res.Error != null && res.Error.length > 0) {
					var params1 = { head: 'Please fix the marked field', body: res.Error, footer: 'Please update and click Submit to continue', color: 'red', selectors: [self.$el.find('#outsidePrincipal')], timeout: '60000' };
					self.fillErrorPopup(params1);
				}
			}); //on success

			request.fail(function() {
				var params = { head: 'Data transmission failed', body: 'if this error returns, please contact support', footer: 'Please try sending again', color: 'red', selectors: [], timeout: '7000' };
				self.fillErrorPopup(params);
			});//on fail
			request.always(function() {
				BlockUi('off');
			});
		}
	},

	addInstallment: function() {
		var date = new Date(moment(this.model.get('Items').last().get('Date')).utc());
		date.setMonth(date.getMonth() + 1);

		var installment = new EzBob.Installment({
			'Editable': false, //true,
			'Deletable': false, //true,
			'Editor': 'Installment',
			'Principal': 0,
			'Balance': 0,
			'BalanceBeforeRepayment': 0,
			'Interest': 0,
			'InterestRate': this.model.get('InterestRate'),
			'Fees': 0,
			'Total': 0,
			'Status': 'StillToPay',
			'Type': 'Installment',
			'IsAdding': true,
			'Date': date
		});
		var editor = this.editors['Installment'];

		var view = new editor({
			model: installment,
			loan: this.model,
		});

		var add = function() {
			this.model.addInstallment(installment);
		};

		view.on('apply', add, this);
		this.showEditView(view);
	}, // addInstallment

	addFee: function() {
		var fee = new EzBob.Installment({
			'Editable': true,
			'Deletable': true,
			'Editor': 'Fee',
			'Principal': 0,
			'Balance': 0,
			'BalanceBeforeRepayment': 0,
			'Interest': 0,
			'InterestRate': this.model.get('InterestRate'),
			'Fees': 0,
			'Total': 0,
			'Type': 'Fee',
			'IsAdding': true,
			'Date': this.model.get('Items').last().get('Date')
		});

		var editor = this.editors['Fee'];

		var view = new editor({
			model: fee,
			loan: this.model,
		});

		var add = function() {
			this.model.addFee(fee);
		};

		view.on('apply', add, this);
		this.showEditView(view);
	}, // addFee

	showEditView: function(view) {
		var self = this;

		view.on('close', function() { self.ui.buttons.show(); });

		this.editRegion.show(view);
		this.ui.buttons.hide();
	}, // showEditView

	removeScheduleItem: function(e) {
		var self = this;
		var id = e.currentTarget.getAttribute('data-id');

		var ok = function() { self.model.removeItem(id); };

		EzBob.ShowMessage('Confirm deleting installment', 'Delete installment', ok, 'Ok', null, 'Cancel');
	}, // removeScheduleItem 

	editScheduleItem: function(e) {
		var id = e.currentTarget.getAttribute('data-id');

		var row = $(e.currentTarget).parents('tr');

		row.addClass('editing');

		var item = this.model.get('Items').at(id);

		this.editItemIndex = id;

		var editor = this.editors[item.get('Editor')];

		var view = new editor({
			model: item,
			loan: this.model,
		});

		view.on('apply', this.recalculate, this);

		var self = this;

		var closed = function() {
			row.removeClass('editing');
			self.editItemIndex = -1;
			self.renderSchedule(this.serializeData());
		};

		view.on('close', closed, this);
		this.showEditView(view);
	}, // editScheduleItem

	recalculate: function() {
		this.model.recalculate();
	}, // recalculate

	onOk: function() {
		var self = this;

		if (this.ui.ok.hasClass('disabled'))
			return;

		var xhr = this.model.save();

		xhr.done(function() {
			self.trigger('item:saved');
			self.close();
		});
	}, // onOk

	onCancel: function() {
		this.close();
	}, // onCancel

	onRender: function() {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.editRegion = new Backbone.Marionette.Region({
			el: this.$('.editloan-item-editor-region')
		});

		this.renderRegions();
		this.setUpView();
	}, // onRender

	setUpView: function() {
		this.$el.find('#fees-calendar-from,#fees-calendar-to,#intrest-calendar-from,#intrest-calendar-to').datepicker({ format: 'dd/mm/yyyy' });
		this.$el.find('#fees-calendar-from,#intrest-calendar-from').datepicker('setDate', new Date());

		var within = this.model.get('ReResultIn');
		if (within.Error != null) {
			if (within.Error === "Within loan arrangement is impossible") {
				this.$el.find('#within-div').css('opacity', '0.5');
				this.$el.find('#radio-1,#withinSelect').attr('disabled', true);
			}
			if (within.Error.indexOf("paid off. Loan balance: ") !== -1) {
				this.$el.find('#within-div,#outside-div').css('opacity', '0.5');
				this.$el.find('#radio-1,#withinSelect,#radio-2,#outsideSelect,#outsidePrincipal').attr('disabled', true);
			}
		}
		this.$el.find('#withinPayments').text(within.IntervalsNum);
		this.$el.find('#withinPrincipal').text(EzBob.formatPounds(within.ReschedulingBalance));
		this.$el.find('#withinIntrest').text(EzBob.formatPounds(within.FirstPaymentInterest));

		var outside = this.model.get('ReResultOut');
		if (outside != null) {
			if (outside.Error != null) {
				if (outside.Error.length > 0) {
					var params = { head: 'Please fix the marked field', body: outside.Error, footer: 'Please update and click Submit to continue', color: 'red', selectors: [this.$el.find('#outsidePrincipal')], timeout: '60000' };
					this.fillErrorPopup(params);
				}
			}
			this.$el.find('#outsidePayments').text(outside.IntervalsNum);
			this.$el.find('#outsidePrincipal').val(outside.DefaultPaymentPerInterval);
			this.$el.find('#outsideIntrest').text(EzBob.formatPounds(outside.FirstPaymentInterest));
		} else {
			this.$el.find('#outsidePayments').text('0');
			this.$el.find('#outsidePrincipal').val('0');
			this.$el.find('#outsideIntrest').text(EzBob.formatPounds('0'));
		}
		var options = this.model.get('Options');
		if (options.AutoPayment === false) {
			this.$el.find('#automatic-charges').prop('checked', true);
			if (options.StopAutoChargeDate != null) {
				this.$el.find('#stop-charges-date').text("stopped from " + EzBob.formatDateWithoutTime(options.StopAutoChargeDate));
			} else {
				this.$el.find('#stop-charges-date').text("permanently stoped.");
			}
			this.$el.find('#stop-charges').show();
		}
		if (options.StopLateFeeFromDate != null && options.StopLateFeeToDate != null) {
			this.$el.find('#fees-calculation').prop('checked', true);
			this.$el.find('#fees-calendar-from').val(EzBob.formatDateWithoutTime(options.StopLateFeeFromDate));
			this.$el.find('#fees-calendar-to').val(EzBob.formatDateWithoutTime(options.StopLateFeeToDate));
		}
	},
	renderRegions: function() {
		var data = this.serializeData();
		this.renderSchedule(data);
		//this.renderFreeze(data);
	}, // renderRegions

	renderSchedule: function(data) {
		this.ui.scheduleEl.html(this.scheduleTemplate(data));
		this.ui.ok.toggleClass('disabled', this.model.get('HasErrors'));
	}, // renderSchedule

	renderFreeze: function(data) {
		this.ui.freezeEl.html(_.template($('#loan_editor_preferences_template').html())(data));
	}, // renderFreeze

	onAddFreezeInterval: function() {
		var sStart = this.$el.find('.new-freeze-interval-start').val();
		var sEnd = this.$el.find('.new-freeze-interval-end').val();
		var nRate = this.$el.find('.new-freeze-interval-rate').val() / 100.0;

		this.$el.find('.new-freeze-interval-error').empty();

		if (this.validateFreezeIntervals(sStart, sEnd))
			this.model.addFreezeInterval(sStart, sEnd, nRate);
		else
			this.$el.find('.new-freeze-interval-error').text('New interval conflicts with one of existing intervals');
	}, // onAddFreezeInterval

	onRemoveFreezeInterval: function(evt) {
		BlockUi('on');
		this.model.removeFreezeInterval(evt.currentTarget.getAttribute('data-id'));
	}, // onRemoveFreezeInterval

	validateFreezeIntervals: function(sStartDate, sEndDate) {
		var self = this;

		var oStart = moment.utc(sStartDate);
		var oEnd = moment.utc(sEndDate);

		if (oStart !== null && oEnd !== null && oStart > oEnd) {
			this.$el.find('.new-freeze-interval-start').val(sEndDate);
			this.$el.find('.new-freeze-interval-end').val(sStartDate);

			var tmp = oEnd;
			oEnd = oStart;
			oStart = tmp;
		} // if

		var bConflict = false;

		_.each(this.model.get('SInterestFreeze'), function(item) {
			if (bConflict)
				return;

			var ary = item.split('|');
			if (ary[4] !== '')
				return;

			var oLeft = moment.utc(ary[0]);
			var oRight = moment.utc(ary[1]);

			var bFirst = self.cmpDates(oStart, oRight);
			var bSecond = self.cmpDates(oLeft, oEnd);

			bConflict = bFirst && bSecond;
		});

		return !bConflict;
	}, // validateFreezeIntervals

	cmpDates: function(a, b) {
		if (a === null || b === null)
			return true;

		return a <= b;
	}, // cmpDates

	onClose: function() {
		return this.editRegion.close();
	}, // onClose

	jqoptions: function() {
		return {
			width: '80%',
			modal: true,
			title: 'Edit Loan Details',
			resizable: true,
		};
	}, // jqoptions
}); // EzBob.EditLoanView
