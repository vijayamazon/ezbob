root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.ApproveLoanForWarningStatusCustomer extends EzBob.BoundItemView
    template: '#approve-loan-for-warning-status-customer'
    
    initialize: (options) ->
        @model = options.model
        @parent = options.parent
        super()
    
    jqoptions: ->
        modal: true
        resizable: false
        title: "Warning"
        position: "center"
        draggable: false
        dialogClass: "warning-customer-status-popup"

    render: ->
        super()
        return @

    serializeData: ->
        m: @model.toJSON()
        
    onSave: ->
        @close()
        @parent.CreateApproveDialog()
        return false
