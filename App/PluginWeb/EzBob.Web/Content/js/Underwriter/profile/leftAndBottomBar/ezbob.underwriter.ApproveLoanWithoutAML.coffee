root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.ApproveLoanWithoutAML extends EzBob.BoundItemView
    template: '#approve-loan-without-aml-template'
    
    initialize: (options) ->
        @model = options.model
        @approveDialog = options.approveDialog
        @skipPopupForApprovalWithoutAML = options.skipPopupForApprovalWithoutAML
        super()
        
    render: ->
        super()
        return @
        
    onSave: ->
        isChecked = $('#isDoNotShowAgain').is(':checked')
        @model.set('SkipPopupForApprovalWithoutAML', isChecked)
        BlockUi "on"
        that = this
        xhr = $.post "#{window.gRootPath}Underwriter/ApplicationInfo/SaveApproveWithoutAML/", {customerId: @model.get('CustomerId'), doNotShowAgain: isChecked}
        xhr.complete -> 
            BlockUi "off"
            that.close()
            that.approveDialog.render()
            return false
