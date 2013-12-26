root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.ApproveLoanWithoutAML extends EzBob.BoundItemView
    template: '#approve-loan-without-aml-template'
    
    initialize: (options) ->
        @model = options.model
        @parent = options.parent
        @skipPopupForApprovalWithoutAML = options.skipPopupForApprovalWithoutAML
        super()
        
    jqoptions: ->
        modal: true
        resizable: false
        title: "Warning"
        position: "center"
        draggable: false
        width: "73%"
        height: Math.max(window.innerHeight * 0.9, 600)
        dialogClass: "warning-aml-status-popup"

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
            that.parent.CheckCustomerStatusAndCreateApproveDialog()
            return false
