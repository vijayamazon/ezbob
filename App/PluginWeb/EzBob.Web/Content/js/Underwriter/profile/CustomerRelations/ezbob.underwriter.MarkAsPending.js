var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarkAsPending = EzBob.BoundItemView.extend({
    template: '#mark-as-pending-template',

    events: {
        
    },

    jqoptions: function () {
        return {
            modal: true,
            resizable: true,
            title: 'CRM - mark as pending',
            position: 'center',
            draggable: true,
            dialogClass: 'customer-relations-popup',
            width: 600
        };
    },

    initialize: function (options) {
        this.actionItems = options.model.get('ActionItems');
	    EzBob.Underwriter.MarkAsPending.__super__.initialize.apply(this, arguments);
    },

    serializeData: function () {
        return {
            actionItems: this.actionItems
        };
    },

    onRender: function () {
        EzBob.Underwriter.MarkAsPending.__super__.onRender.apply(this, arguments);
    },

    ui: {
    },

    onSave: function () {
        var self = this;

        BlockUi();
        
        var xhr = $.post(window.gRootPath + 'CustomerRelations/MarkAsPending', { customerId: this.model.customerId });
        xhr.always(function () {
            // make other button visible and this invisible - if i return success
            // or refresh model?
            return UnBlockUi();
        });

        self.close();

        return false;
    },
});
