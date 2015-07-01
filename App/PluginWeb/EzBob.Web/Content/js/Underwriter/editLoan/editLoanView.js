var EzBob = EzBob || {};

EzBob.EditLoanView = Backbone.Marionette.ItemView.extend({
    template: '#loan_editor_template',

    scheduleTemplate: $('#loan_editor_schedule_template').length > 0 ? _.template($('#loan_editor_schedule_template').html()) : null,

    initialize: function () {
        this.bindTo(this.model, 'change sync', this.renderRegions, this);
        this.modelBinder = new Backbone.ModelBinder();
        this.editItemIndex = -1;
    }, // initialize
    bindings: {
        //WithinWeek: "span[name=\"withinWeek\"]",
        //WithinMonth: "span[name=\"withinMonth\"]",
        //ReschedulingBalance: "span[name=\"withinAmount\"]",
        //InterestRate: "span[name=\"Intrest\"]",
        //OutsideWeek: "span[name=\"outsideWeek\"]",
        //OutsideMonth: "span[name=\"outsideMonth\"]"
    },//bindings
    serializeData: function () {
        var data = this.model.toJSON();
        data.editItemIndex = this.editItemIndex;
        data.hasFreezInterest = this.hasFreeIntrest();
        return data;
    }, // serializeData

    hasFreeIntrest: function () {
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
        data_error: '#data-error',
        amount_error: '#amount-error',
        date_empty_intrest: '#date-empty-intrest',
        date_error_intrest: '#date-error-intrest',
        date_empty_fees: '#date-empty-fees',
        date_error_fees: '#date-error-fees',
        selection_error: '#selection-error',
        submit_success: '#submit-success',
        submit_fail: '#submit-fail'
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
        'click .add-installment': 'addInstallment',
        'click .add-fee': 'addFee',
        'click .save': 'onOk',
        'click .cancel': 'onCancel',
        'click .add-freeze-interval': 'onAddFreezeInterval',
        'click .remove-freeze-interval': 'onRemoveFreezeInterval'
    }, // events

    withinSelectChange: function() {
        var duration = $('#withinSelect option:selected').text();
        var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: duration, rescheduleIn: 'true' });
        var self = this;
        request.success(function (res) {
            $('#withinPayments').text(res.IntervalsNum + " " + duration);
            $('#withinPrincipal').text(EzBob.formatPounds(res.ReschedulingBalance));
            $('#withinIntrest').text(EzBob.formatPounds(res.FirstPaymentInterest));
        }); //on success

        request.fail(function () {
            self.ui.data_error.fadeIn().fadeOut(3000);
        });//on fail
    },
    outsideSelectChange: function() {
        $('#ReschedulingOUTNotification').hide();
        var amount = $("#outsidePrincipal").val();
        var duration = $('#outsideSelect option:selected').text();
        if (typeof amount === 'undefined' || amount <= 0) {
            this.ui.amount_error.fadeIn().fadeOut(3000);
        } else {
            var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: duration, AmountPerInterval: amount, rescheduleIn: 'false' });
            var self = this;
            request.success(function (res) {
                $('#outsidePayments').text(res.IntervalsNum + " " + duration);
                $('#outsideIntrest').text(EzBob.formatPounds(res.FirstPaymentInterest));
                if (res.Error != null && res.Error.length > 0) {
                    $('#ReschedulingOUTNotification').text(res.Error);
                    $('#ReschedulingOUTNotification').fadeIn();
                }
            }); //on success

            request.fail(function () {
                self.ui.data_error.fadeIn().fadeOut(3000);
            });//on fail
        }
    },
    reschSubmitForm: function () {

        var requestParam = { loanID: this.model.get('Id'), save: 'true' };

        var checkedRadio = $('input[name=rescheduleIn]').filter(':checked').val();
        if (checkedRadio === 'true') {
            requestParam.intervalType = $('#withinSelect option:selected').text();
            requestParam.rescheduleIn = 'true';
        }
        else if (checkedRadio === 'false') {
            requestParam.intervalType = $('#outsideSelect option:selected').text();
            requestParam.AmountPerInterval = $('#outsideAmount').val();
            requestParam.rescheduleIn = 'false';
        } else {
            this.ui.selection_error.fadeIn().fadeOut(3000);
            return false;
        }

        var isStopCharges = $('#automatic-charges').is(':checked');
        var isStopIntrest = $("#intrest-checkbox").is(':checked');
        var isStopFees = $('#fees-calculation').is(':checked');

        var chargesVal = $('#charges-payments').val();
        var interestFrom = EzBob.formatDateTimeCS($('#intrest-calendar-from').val());
        var interestTo = EzBob.formatDateTimeCS($('#intrest-calendar-to').val());
        var feesFrom = EzBob.formatDateTimeCS($('#fees-calendar-from').val());
        var feesTo = EzBob.formatDateTimeCS($('#fees-calendar-to').val());

        if (chargesVal < 0 || chargesVal === "") {
            $('#charges-error').fadeIn().fadeOut(3000);
            return false;
        }
        if (isStopCharges) {
            requestParam.stopAutoCharge = 'true';
            requestParam.stopAutoChargePayment = chargesVal;
        }
        if (isStopIntrest) {
            if (interestTo === "" || interestFrom === "") {
                this.ui.date_empty_intrest.fadeIn().fadeOut(3000);
                return false;
            }
            if (new Date(interestFrom).getTime() > new Date(interestTo).getTime()) {
                this.ui.date_error_intrest.fadeIn().fadeOut(3000);
                return false;
            }
            requestParam.freezeInterest = 'true';
            requestParam.freezeStartDate = interestFrom;
            requestParam.freezeEndDate = interestTo;
        }
        if (isStopFees) {
            if (feesTo === "" || feesFrom === "") {
                this.ui.date_empty_fees.fadeIn().fadeOut(3000);
                return false;
            }
            if (new Date(feesFrom).getTime() > new Date(feesTo).getTime()) {
                this.ui.date_error_fees.fadeIn().fadeOut(3000);
                return false;
            }
            requestParam.stopLateFee = 'true';
            requestParam.lateFeeStartDate = feesFrom;
            requestParam.lateFeeEndDate = feesTo;
        }

        var oRequest = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', requestParam);

        var self = this;

        oRequest.success(function(res) {
            if (res.Error == null || res.Error === "") {
                self.ui.submit_success.fadeIn().fadeOut(3000);
                setTimeout(function() { self.close(); }, 3500);
            } else {
                self.ui.submit_fail.text(res.Error);
                self.ui.submit_fail.fadeIn().fadeOut(10000);
            }
            return false;
        }); //on success

        oRequest.fail(function () {
            self.ui.submit_fail.text("There has been an error sending data to server");
            self.ui.submit_fail.fadeIn().fadeOut(3000);
            return false;
        });//on fail
    },

    onChangeAmount: function () {
        $('#ReschedulingOUTNotification').hide();
        var amount = $("#outsidePrincipal").val();
        var duration = $('#outsideSelect option:selected').text();
        if (typeof amount === 'undefined' || amount <= 0) {
            this.ui.amount_error.fadeIn().fadeOut(3000);
        } else {
            var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: duration, AmountPerInterval: amount, rescheduleIn: 'false' });
            var self = this;
            request.success(function (res) {
                $('#outsidePayments').text(res.IntervalsNum + " " + duration);
                $('#outsideIntrest').text(EzBob.formatPounds(res.FirstPaymentInterest));
                if (res.Error!=null && res.Error.length > 0) {
                    $('#ReschedulingOUTNotification').text(res.Error);
                    $('#ReschedulingOUTNotification').fadeIn();
                }
            }); //on success

            request.fail(function () {
                self.ui.data_error.fadeIn().fadeOut(3000);
            });//on fail
        }
    },

    addInstallment: function () {
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

        var add = function () {
            this.model.addInstallment(installment);
        };

        view.on('apply', add, this);
        this.showEditView(view);
    }, // addInstallment

    addFee: function () {
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

        var add = function () {
            this.model.addFee(fee);
        };

        view.on('apply', add, this);
        this.showEditView(view);
    }, // addFee

    showEditView: function (view) {
        var self = this;

        view.on('close', function () { self.ui.buttons.show(); });

        this.editRegion.show(view);
        this.ui.buttons.hide();
    }, // showEditView

    removeScheduleItem: function (e) {
        var self = this;
        var id = e.currentTarget.getAttribute('data-id');

        var ok = function () { self.model.removeItem(id); };

        EzBob.ShowMessage('Confirm deleting installment', 'Delete installment', ok, 'Ok', null, 'Cancel');
    }, // removeScheduleItem 

    editScheduleItem: function (e) {
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

        var closed = function () {
            row.removeClass('editing');
            self.editItemIndex = -1;
            self.renderSchedule(this.serializeData());
        };

        view.on('close', closed, this);
        this.showEditView(view);
    }, // editScheduleItem

    recalculate: function () {
        this.model.recalculate();
    }, // recalculate

    onOk: function () {
        var self = this;

        if (this.ui.ok.hasClass('disabled'))
            return;

        var xhr = this.model.save();

        xhr.done(function () {
            self.trigger('item:saved');
            self.close();
        });
    }, // onOk

    onCancel: function () {
        this.close();
    }, // onCancel

    onRender: function () {
        this.modelBinder.bind(this.model, this.el, this.bindings);
        this.editRegion = new Backbone.Marionette.Region({
            el: this.$('.editloan-item-editor-region')
        });

        this.renderRegions();
        this.setUpView();
    }, // onRender

    setUpView: function() {
        this.$el.find('#fees-calendar-from').datepicker('setDate', new Date());
        this.$el.find('#fees-calendar-to').datepicker();
        this.$el.find('#intrest-calendar-from').datepicker('setDate', new Date());
        this.$el.find('#intrest-calendar-to').datepicker();

        var within = this.model.get('ReResultIn');
        if (within.Error != null && within.Error.length > 0) {
            this.$el.find('#ReschedulingINNotification').text(within.Error);
            this.$el.find('#ReschedulingINNotification').show();
            this.$el.find('#exception-div').hide();
            this.$el.find('#radio-2').prop('checked', true);
        }
        this.$el.find('#withinPayments').text(within.IntervalsNum + " Month");
        this.$el.find('#withinPrincipal').text(EzBob.formatPounds(within.ReschedulingBalance));
        this.$el.find('#withinIntrest').text(EzBob.formatPounds(within.FirstPaymentInterest));

        var outside = this.model.get('ReResultOut');
        if (outside.Error != null && outside.Error.length > 0) {
            this.$el.find('#ReschedulingOUTNotification').text(outside.Error);
            this.$el.find('#ReschedulingOUTNotification').show();
        }
        this.$el.find('#outsidePayments').text(outside.IntervalsNum + " Month");
        this.$el.find('#outsidePrincipal').val(outside.DefaultPaymentPerInterval);
        this.$el.find('#outsideIntrest').text(EzBob.formatPounds(outside.FirstPaymentInterest));

        var options = this.model.get('Options');
        if (options.AutoPayment === false) {
            this.$el.find('#automatic-charges').prop('checked', true);
            if (options.StopAutoChargeDate != null) {
                this.$el.find('#stop-charges').show();
                this.$el.find('#stop-charges-date').text(EzBob.formatDateWithoutTime(options.StopAutoChargeDate));
            }
        }
        if (options.StopLateFeeFromDate != null && options.StopLateFeeToDate != null) {
            this.$el.find('#fees-calculation').prop('checked', true);
            this.$el.find('#fees-calendar-from').val(EzBob.formatDateWithoutTime(options.StopLateFeeFromDate));
            this.$el.find('#fees-calendar-to').val(EzBob.formatDateWithoutTime(options.StopLateFeeToDate));
        }
    },
    renderRegions: function () {
        var data = this.serializeData();
        this.renderSchedule(data);
        //this.renderFreeze(data);
    }, // renderRegions

    renderSchedule: function (data) {
        this.ui.scheduleEl.html(this.scheduleTemplate(data));
        this.ui.ok.toggleClass('disabled', this.model.get('HasErrors'));
    }, // renderSchedule

    renderFreeze: function (data) {
        this.ui.freezeEl.html(_.template($('#loan_editor_preferences_template').html())(data));
    }, // renderFreeze

    onAddFreezeInterval: function () {
        var sStart = this.$el.find('.new-freeze-interval-start').val();
        var sEnd = this.$el.find('.new-freeze-interval-end').val();
        var nRate = this.$el.find('.new-freeze-interval-rate').val() / 100.0;

        this.$el.find('.new-freeze-interval-error').empty();

        if (this.validateFreezeIntervals(sStart, sEnd))
            this.model.addFreezeInterval(sStart, sEnd, nRate);
        else
            this.$el.find('.new-freeze-interval-error').text('New interval conflicts with one of existing intervals');
    }, // onAddFreezeInterval

    onRemoveFreezeInterval: function (evt) {
        this.model.removeFreezeInterval(evt.currentTarget.getAttribute('data-id'));
    }, // onRemoveFreezeInterval

    validateFreezeIntervals: function (sStartDate, sEndDate) {
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

        _.each(this.model.get('SInterestFreeze'), function (item) {
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

    cmpDates: function (a, b) {
        if (a === null || b === null)
            return true;

        return a <= b;
    }, // cmpDates

    onClose: function () {
        return this.editRegion.close();
    }, // onClose

    jqoptions: function () {
        return {
            width: '80%',
            modal: true,
            title: 'Edit Loan Details',
            resizable: true,
        };
    }, // jqoptions
}); // EzBob.EditLoanView
