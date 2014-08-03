root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.RolloverView extends Backbone.View
    initialize: ->
        @template = _.template $('#payment-rollover-template').html()
        @model.hasActive = false
        for val in @model.rollover
            if val.Status is 0
                @model.hasActive = true
                @model.rollover = val
                return null
        @

    jqoptions: ->
        modal: true
        resizable: false
        title: "Rollover"
        position: "center"
        draggable: false
        dialogClass: "rollover-popup"
        width: 600

    events:
        "click .confirm" : "addRollover",
        "click .remove" : "removeRollover"
        "change [name='ExperiedDate']" : "updatePaymentData"

    render: ->
        @model.title = if not @model.hasActive then "Add roll over" else "Edit roll over"
        @$el.html @template model: @model
        @$el.find('.ezDateTime').splittedDateTime()
        @form = @$el.find "#rollover-dialog"
        @validator = EzBob.validateRollover @form
        SetDefaultDate @$el.find('#ExperiedDate'), @model.rollover.ExpiryDate if @model.hasActive
        @$el.find('select[name=\"ScheduleId\"]').change()
        @updatePaymentData()
        @

    addRollover: ->
        unless this.validator.form() 
            return false

        # Find disabled inputs, and remove the "disabled" attribute
        disabled = @form.find(':input:disabled').removeAttr('disabled');

        #serialize the form
        @trigger "addRollover", @form.serializeArray()

        #re-disabled the set of inputs that you previously enabled
        disabled.attr('disabled','disabled');
        true

    removeRollover: ->
        rolloverId = @$el.find('input[name=\"rolloverId\"]');
        @trigger "removeRollover", rolloverId

    updatePaymentData: ->
        data = {
            loanId: @model.loanId
            isEdit: @model.hasActive
        }
        request = $.get window.gRootPath + "Underwriter/LoanHistory/GetRolloverInfo", data
        request.done (r) =>
            return if r.error
            @$el.find("#Payment").val r.rolloverAmount
            @$el.find("#interest").val r.interest
            @$el.find("#lateFees").val r.lateCharge
            @$el.find("#MounthCount").val r.mounthAmount
