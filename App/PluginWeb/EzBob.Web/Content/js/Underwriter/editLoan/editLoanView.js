var EzBob = EzBob || {};

EzBob.EditLoanView = Backbone.Marionette.ItemView.extend({
	template: '#loan_editor_template',

	scheduleTemplate: $('#loan_editor_schedule_template').length > 0 ? _.template($('#loan_editor_schedule_template').html()) : null,

	initialize: function() {
		this.bindTo(this.model, 'change sync', this.renderRegions, this);
		this.editItemIndex = -1;
	}, // initialize

	serializeData: function() {
		var data = this.model.toJSON();
		data.editItemIndex = this.editItemIndex;
		return data;
	}, // serializeData

	ui: {
		scheduleEl: '.editloan-schedule-region',
		freezeEl: '.editloan-freeze-intervals-region',
		ok: '.save',
		buttons: '.buttons'
	}, // ui

	editors: {
		'Installment': EzBob.InstallmentEditor,
		'Fee': EzBob.FeeEditor,
	}, // editors

	events: {
	    'click .submit-btn': 'submitForm',
	    'blur .outside-weeks-amount': 'blurWeeks',
	    'blur .outside-month-amount': 'blurMonth',
		'click .edit-schedule-item': 'editScheduleItem',
		'click .remove-schedule-item': 'removeScheduleItem',
		'click .add-installment': 'addInstallment',
		'click .add-fee': 'addFee',
		'click .save': 'onOk',
		'click .cancel': 'onCancel',
		'click .add-freeze-interval': 'onAddFreezeInterval',
		'click .remove-freeze-interval': 'onRemoveFreezeInterval'
	}, // events

	submitForm: function() {
	    alert("submit");
	},

    blurWeeks: function() {
        if ($(".outside-weeks-amount").val() <= 0) {
            $("#error").fadeIn();
            setTimeout(function () { $("#error").fadeOut(); }, 3000);
        }
    },

    blurMonth: function() {
        alert($(".outside-month-amount").val());
    },

	addInstallment: function() {
		var date = new Date(moment(this.model.get('Items').last().get('Date')).utc());
		date.setMonth(date.getMonth() + 1);

		var installment = new EzBob.Installment({
			'Editable': true,
			'Deletable': true,
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
		this.editRegion = new Backbone.Marionette.Region({
			el: this.$('.editloan-item-editor-region')
		});

		this.renderRegions();
	}, // onRender

	renderRegions: function() {
		var data = this.serializeData();
		this.renderSchedule(data);
		this.renderFreeze(data);
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

		_.each(this.model.get('InterestFreeze'), function(item) {
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
