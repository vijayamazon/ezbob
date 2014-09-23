var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AddCustomerRelationsFollowUp = EzBob.BoundItemView.extend({
    template: '#add-customer-relations-followup-template',

    events: {
        'keyup #Comment': 'commentKeyup'
    }, // events

    jqoptions: function () {
        return {
            modal: true,
            resizable: true,
            title: 'CRM - add follow up',
            position: 'center',
            draggable: true,
            dialogClass: 'customer-relations-popup',
            width: 600
        };
    }, // jqoptions

    initialize: function (options) {
        this.onsave = options.onsave;
        this.onbeforesave = options.onbeforesave;
        this.customerId = this.model.customerId;
        this.url = window.gRootPath + 'Underwriter/CustomerRelations/SaveFollowUp/';
	    this.isBroker = options.isBroker;

        EzBob.Underwriter.AddCustomerRelationsFollowUp.__super__.initialize.apply(this, arguments);
    }, // initialize

    onRender: function () {
        EzBob.Underwriter.AddCustomerRelationsFollowUp.__super__.onRender.apply(this, arguments);

        var that = this;
        this.ui.FollowUpDate.datepicker({ format: 'dd/mm/yyyy', autoclose: true, keyboardNavigation: true });
        this.ui.FollowUpDate.on('changeDate', function () {
            that.ui.Comment.focus();
        });
    }, // onRender

    commentKeyup: function (el) {
        return this.ui.Comment.val(this.ui.Comment.val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000));
    }, // commentKeyup

    ui: {
        FollowUpDate: '#FollowUpDate',
        Comment: '#Comment',
    }, // ui

    onSave: function () {
        if (!this.ui.FollowUpDate.val())
            return false;

        if (!this.ui.Comment.val())
            return false;

        BlockUi();

        var opts = {
            followUpDate: EzBob.formatDateTimeCS(moment(this.ui.FollowUpDate.val(), 'DD/MM/YYYY')),
            comment: this.ui.Comment.val(),
            customerId: this.customerId,
            isBroker: this.isBroker,
        };

        if (this.onbeforesave)
            this.onbeforesave(opts);

        var self = this;

        var xhr = $.post(this.url, opts);

        xhr.done(function (r) {
            if (r.success)
                self.model.fetch();
            else {
                if (r.error)
                    EzBob.ShowMessage(r.error, 'Error');
            } // if

            self.close();
        });

        xhr.always(function () {
            return UnBlockUi();
        });

        return false;
    }, // onSave
}); // EzBob.Underwriter.AddCustomerRelationsFollowUp
