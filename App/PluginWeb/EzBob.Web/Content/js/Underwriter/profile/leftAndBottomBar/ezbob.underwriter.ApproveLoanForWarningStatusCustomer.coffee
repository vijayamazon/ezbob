root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.ApproveLoanForWarningStatusCustomer extends EzBob.BoundItemView
    template: '#approve-loan-for-warning-status-customer'
    
    initialize: (options) ->
        @model = options.model
        @parent = options.parent
        super()
        
    render: ->
        super()
        return @
        
    onSave: ->
        @close()
        @parent.CreateApproveDialog()
        return false
