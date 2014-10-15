var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarkAsPending = EzBob.BoundItemView.extend({
    template: '#mark-as-pending-template',

    events: {
        'click .checkbox': 'checkboxClicked'
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
        this.actionItemsSavedCallback = options.actionItemsSavedCallback;
	    EzBob.Underwriter.MarkAsPending.__super__.initialize.apply(this, arguments);
    },

    serializeData: function () {
        return {
            actionItems: this.actionItems
        };
    },

    checkboxClicked: function (e) {
        var actionItems = this.model.get('ActionItems');
        var clickedId = e.currentTarget.id.substring(19);
        for (var i = 0; i < actionItems.length; i++) {
            var ai = actionItems[i];
            if (ai.Id == clickedId) {
                ai.IsChecked = !ai.IsChecked;
                break;
            }
        }
    },

    onRender: function () {
        EzBob.Underwriter.MarkAsPending.__super__.onRender.apply(this, arguments);
    },

    onSave: function () {
        var self = this;

        BlockUi();
        
        var actionItemsAsString = '';
        var actionItems = this.model.get('ActionItems');
        for (var i = 0; i < actionItems.length; i++) {
            var ai = actionItems[i];
            if (ai.IsChecked) {
                actionItemsAsString += ',' + ai.Id;
            }
        }
        var xhr = $.post(window.gRootPath + 'CustomerRelations/MarkAsPending', { customerId: this.model.customerId, actionItems: actionItemsAsString });
        xhr.always(function () {
            self.actionItemsSavedCallback();
            return UnBlockUi();
        });

        self.close();

        return false;
    },
});
