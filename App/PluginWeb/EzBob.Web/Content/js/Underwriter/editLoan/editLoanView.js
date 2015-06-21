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
        WithinWeek: "span[name=\"withinWeek\"]",
        WithinMonth: "span[name=\"withinMonth\"]",
        ReschedulingBalance: "span[name=\"withinAmount\"]",
        InterestRate: "span[name=\"Intrest\"]",
        OutsideWeek: "span[name=\"outsideWeek\"]",
        OutsideMonth: "span[name=\"outsideMonth\"]"
    },
    serializeData: function () {
        var data = this.model.toJSON();
        data.editItemIndex = this.editItemIndex;
        data.hasFreezInterest = this.hasFreeIntrest();
        return data;
    }, // serializeData

    hasFreeIntrest: function () {
        var freezArray = this.model.get('InterestFreeze');
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

    },

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
        within_selector_error: '#within-selector-error',
        outside_selector_error: '#outside-selector-error',
        submit_success: '#submit-success',
        submit_fail: '#submit-fail'
    }, // ui

    editors: {
        'Installment': EzBob.InstallmentEditor,
        'Fee': EzBob.FeeEditor,
    }, // editors

    events: {
        'click #resch-submit-btn': 'reschSubmitForm',
        'blur #outsideAmount': 'onChangeAmount',
        'click .option-btn': 'fillData',
        'click .edit-schedule-item': 'editScheduleItem',
        'click .remove-schedule-item': 'removeScheduleItem',
        'click .add-installment': 'addInstallment',
        'click .add-fee': 'addFee',
        'click .save': 'onOk',
        'click .cancel': 'onCancel',
        'click .add-freeze-interval': 'onAddFreezeInterval',
        'click .remove-freeze-interval': 'onRemoveFreezeInterval'
    }, // events

    fillData: function () {
        var colapseState = $('#collapsable-content').is(':visible');
        $("#collapsable-content").slideToggle('slow');
        
        if (!colapseState) {
            var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: "Month", rescheduleIn: 'true' });

            var self = this;

            request.success(function(res) {
                self.model.set('WithinWeek', res.IntervalsNumWeeks);
                self.model.set('WithinMonth', res.IntervalsNum);
                self.model.set('ReschedulingBalance', res.ReschedulingBalance);
                
                if (res.Error === "ReschedulingInPeriodException") {
                    $('#rescheduleIn-true').attr('disabled', true);
                    $('#within-loan-period-text').css('opacity', 0.8);
                    $('#within-loan-period-text').css('cursor', 'not-allowed');
                    $('#withinSelect').attr('disabled', true);
                }
            }); //on success

            request.fail(function() {
                self.ui.data_error.fadeIn().fadeOut(3000);
            }); //on fail
        }
    },
    reschSubmitForm: function () {

        var requestParam = { loanID: this.model.get('Id'), save: 'true' };

        var checkedRadio = $('input[name=rescheduleIn]').filter(':checked').val();
        if (checkedRadio === 'true') {
            var inSelector = $('#withinSelect option:selected').text();
            if (inSelector === 'Week' || inSelector === 'Month')
                requestParam.intervalType = inSelector;
            else {
                this.ui.within_selector_error.fadeIn().fadeOut(3000);
                return false;
            }
            requestParam.rescheduleIn = 'true';
        }
        else if (checkedRadio === 'false') {
            var outSelector = $('#outsideSelect option:selected').text();
            if (outSelector === 'Week' || outSelector === 'Month')
                requestParam.intervalType = outSelector;
            else {
                this.ui.outside_selector_error.fadeIn().fadeOut(3000);
                return false;
            }
            requestParam.AmountPerInterval = $('#outsideAmount').val();
            requestParam.rescheduleIn = 'false';
            var isStopCharges = $('#charges-checkbox').is(':checked');
            var chargesVal = $('#charges-payments').val();
            if (chargesVal <= 0 || chargesVal === "")
                return false;
            if (isStopCharges) {
                requestParam.stopAutoCharge = 'true';
                requestParam.stopAutoChargePayment = chargesVal;
            }
        } else {
            this.ui.selection_error.fadeIn().fadeOut(3000);
            return false;
        }

        var isStopIntrest = $("#intrest-checkbox").is(':checked');
        var isStopFees = $('#fees-checkbox').is(':checked');

        var interestFrom = $('#intrest-calendar-from').val();
        var interestTo = $('#intrest-calendar-to').val();
        var feesFrom = $('#fees-calendar-from').val();
        var feesTo = $('#fees-calendar-to').val();

        if (isStopIntrest) {
            if (interestTo === "" || interestFrom === "") {
                this.ui.data_error.fadeIn().fadeOut(3000);
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

        oRequest.success(function (res) {
            self.ui.submit_success.fadeIn().fadeOut(3000);
        }); //on success

        oRequest.fail(function () {
            self.ui.submit_fail.fadeIn().fadeOut(3000);
        });//on fail
    },

    onChangeAmount: function () {
        var amount = $("#outsideAmount").val();
        if (typeof amount === 'undefined' || amount <= 0) {
            this.ui.amount_error.fadeIn().fadeOut(3000);
        } else {
            var request = $.post('' + window.gRootPath + 'Underwriter/LoanEditor/RescheduleLoan/', { loanID: this.model.get('Id'), intervalType: "Month", AmountPerInterval: amount, rescheduleIn: 'false' });
            var self = this;
            request.success(function (res) {
                self.model.set('OutsideWeek', res.IntervalsNumWeeks);
                self.model.set('OutsideMonth', res.IntervalsNum);
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
        //console.log(this.model);
        this.editRegion = new Backbone.Marionette.Region({
            el: this.$('.editloan-item-editor-region')
        });

        this.renderRegions();

        this.$el.find('#fees-calendar-from').datepicker();
        this.$el.find('#fees-calendar-to').datepicker();
        this.$el.find('#intrest-calendar-from').datepicker();
        this.$el.find('#intrest-calendar-to').datepicker();
        
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
    }, // onRender

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

        _.each(this.model.get('InterestFreeze'), function (item) {
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
