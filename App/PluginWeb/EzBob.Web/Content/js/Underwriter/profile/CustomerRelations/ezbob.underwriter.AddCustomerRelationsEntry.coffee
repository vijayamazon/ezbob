root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.AddCustomerRelationsEntry extends EzBob.BoundItemView
    template: '#add-customer-relations-entry-template'

    events:
        "keyup textarea": "commentKeyup"

    jqoptions: ->
        modal: true
        resizable: false
        title: "CRM"
        position: "center"
        draggable: false
        dialogClass: "customer-relations-popup"
        width: 600

    initialize: (options) ->
        @model = new Backbone.Model(actions: options.actions, statuses: options.statuses)
        @mainTab = options.mainTab
        super()
        
    render: ->
        super()
        @$el.find('#Action').prop('selectedIndex',1)
        return @

    commentKeyup: (el) ->
        $(el.target).val($(el.target).val().replace(/\r\n|\r|\n/g, "\r\n").slice(0, 1000))
        
    onSave: ->
        return false if (!$('#Incoming_I')[0].checked && !$('#Incoming_O')[0].checked)
        return false if ($('#Status')[0].selectedIndex == 0)
        return false if ($('#Action')[0].selectedIndex == 0)
        
        BlockUi "on" 
        that = this
        xhr = $.post "#{window.gRootPath}Underwriter/CustomerRelations/SaveEntry/", {isIncoming: $('#Incoming_I')[0].checked, action: $('#Action')[0].value, status: $('#Status')[0].value, comment: $('#Comment').val(), customerId: this.mainTab.model.customerId}
        xhr.done (r) =>
            if r.error?
                EzBob.ShowMessage(r.error, "Error")
                return
            that.mainTab.model.fetch()
            @close()
        xhr.complete -> BlockUi "off"
        return false
