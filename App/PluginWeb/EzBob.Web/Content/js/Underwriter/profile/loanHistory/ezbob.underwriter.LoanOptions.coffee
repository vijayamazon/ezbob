root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.LoanOptionsModel extends Backbone.Model
    urlRoot: -> "#{window.gRootPath}Underwriter/LoanOptions/Index?loanId=#{@loanId}"

class EzBob.Underwriter.LoanOptionsView extends Backbone.Marionette.ItemView
    template: '#loan-options-template'

    jqoptions: ->
        modal: true
        resizable: false
        title: "Loan Options"
        position: "center"
        draggable: false
        width: "73%"
        height: Math.max(window.innerHeight * 0.9, 600)
        dialogClass: "loan-options-popup"

    initialize: () ->
        @loanOptions = new Backbone.Model(@model.get('Options'))
        @modelBinder = new Backbone.ModelBinder()
        @

    bindings:
        Id:
            selector: "input[name='Id']"
        LoanId:
            selector: "input[name='LoanId']"
        AutoPayment:
            selector: "input[name='AutoPayment']"
        ReductionFee:
            selector: "input[name='ReductionFee']"
        LatePaymentNotification:
            selector: "input[name='LatePaymentNotification']"
        StopSendingEmails:
            selector: "input[name='StopSendingEmails']"

     events: 
        'change #cais-flags' : 'changeFlags'
        'change #CaisAccountStatus': 'changeAccountStatus'
        "click .btnOk":"onSave"

    changeFlags: ->
        @loanOptions.set('ManulCaisFlag', @$el.find("#cais-flags option:selected").val() ) 
        index = @$el.find("#cais-flags option:selected").attr('data-id')
        curentFlag = @model.get('ManualCaisFlags')[index]
        @$el.find('.cais-comment').html('<h5>'+curentFlag.ValidForRecordType+'</h5>' + curentFlag.Comment)

    changeAccountStatus:=>
        tmp = $("#CaisAccountStatus option:selected").val()
        if tmp == '8' 
            $("#defaultExplanation").show()
        else
            $("#defaultExplanation").hide()
        @loanOptions.set('CaisAccountStatus', $("#CaisAccountStatus option:selected").val() ) 

    save: ->
        postData = @loanOptions.toJSON()
        action = "#{window.gRootPath}Underwriter/LoanOptions/Save"
        request = $.post(action, postData);
        false
    
    onCancel: =>
        @close()
    
    onSave: =>
        @save()
        @close()
 
    onRender: ->
        @modalOptions = 
            show: true, 
            keyboard: false 
            width: 700
        @modelBinder.bind @loanOptions, @el, @bindings
        @$el.find("#CaisAccountStatus option[value='#{@loanOptions.get 'CaisAccountStatus'}']").attr('selected', 'selected')
        @$el.find("#cais-flags option[value='#{@loanOptions.get 'ManulCaisFlag'}']").attr('selected', 'selected')
        @changeFlags()