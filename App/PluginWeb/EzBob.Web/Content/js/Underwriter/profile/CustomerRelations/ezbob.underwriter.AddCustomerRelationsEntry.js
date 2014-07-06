var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AddCustomerRelationsEntry = EzBob.BoundItemView.extend({
    template: '#add-customer-relations-entry-template',

    events: {
        'keyup #Comment': 'commentKeyup',
    }, // events
    jqoptions: function () {
        return {
            modal: true,
            resizable: true,
            title: 'CRM - add entry',
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
        this.url = options.url;

        EzBob.Underwriter.AddCustomerRelationsEntry.__super__.initialize.call(this);
    }, // initialize

    onRender: function () {
        this.ui.Action.prop('selectedIndex', 1);
        var rank = this.model.get('CurrentRank');
        if (rank) {
            this.ui.Rank.val(rank.Id);
            this.ui.RankSpan.addClass('active attardi-has-data');
        }
        
        if(this.model.get('isBroker')) {
            this.ui.RankDiv.hide();
        }
    }, // onRender

    serializeData: function () {
        return {
            actions: EzBob.CrmActions,
            statuses: EzBob.CrmStatuses,
            ranks: EzBob.CrmRanks,
        };
    }, // serializeData

    commentKeyup: function (el) {
        return this.ui.Comment.val(this.ui.Comment.val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000));
    }, // commentKeyup

    ui: {
        Type: 'input[name="Type"]:checked',
        Status: '#Status',
        Action: '#Action',
        Rank: '#Rank',
        RankSpan: '.rank-span',
        RankDiv: '.rank-div',
        Comment: '#Comment',
    }, // ui

    onSave: function () {
        if (this.ui.Status[0].selectedIndex === 0)
            return false;

        if (this.ui.Action[0].selectedIndex === 0)
            return false;

        BlockUi();
        var opts = {
            type: this.$el.find('input[name="Type"]:checked').data("value"),
            action: this.ui.Action[0].value,
            status: this.ui.Status[0].value,
            rank: this.ui.Rank[0].value,
            comment: this.ui.Comment.val(),
            customerId: this.customerId
        };

        if (this.onbeforesave)
            this.onbeforesave(opts);

        var self = this;

        var xhr = $.post(this.url, opts);

        xhr.done(function (r) {
            if (r.success && !self.model.get('isBroker')) {
                self.model.fetch();
            }else if (r.success && self.onsave) {
                self.onsave();
            } else {
                if (r.error) {
                    EzBob.ShowMessage(r.error, 'Error');
                }
            } // if

            self.close();
        });

        xhr.always(function () {
            return UnBlockUi();
        });

        return false;
    }, // onSave
}); // EzBob.Underwriter.AddCustomerRelationsEntry
