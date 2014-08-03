root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.LoanHistoryDetailsModel extends Backbone.Model
        url: -> "#{window.gRootPath}Underwriter/LoanHistory/Details?customerid=#{@id}&loanid=#{@loanid}"

class EzBob.Underwriter.LoanDetailsView extends Backbone.Marionette.View
    initialize: ->
        @template = _.template($("#loan-details-template").html())
        @bindTo @model, "change reset", @render, this

    attributes:
        class: "underwriter-loan-details"

    render: ->
        that = this
        @$el.dialog
            modal: true
            resizable: true
            title: "Loan Details - #{@options.loan.RefNumber}"
            position: "center"
            draggable: true
            width: "1000"
            height: "900"
            close: ->
                $(this).dialog "destroy"
                that.trigger "close"

        @renderContent()
        this

    renderContent: ->
        modelLoan = @options.loan
        model = @model.toJSON()
        details = model.details

        @$el.html @template(
            loan: modelLoan
            transactions: details and details.Transactions
            schedule: details and details.Schedule
            pacnetTransactions: details and details.PacnetTransactions
            area: "Underwriter"
            rollovers: details and details.Rollovers
            charges: details and details.Charges
            showFailed: @$el.find('.filter-errors').is(':checked')
            rolloverCount: model.rolloverCount
        )
        if modelLoan.Modified
            @$el.find('.offer-status').append("<strong>Loan was manually modified</strong>").css({"margin-top": "-20px"});

    events:
        "click .rollover": "rollover"
        "click .make-payment": "makePayment"
        "click #btn-options": "showDialogOptions"
        "change .filter-errors": "renderContent"
        "click .pdf-link": "exportToPdf"
        "click .excel-link": "exportToExcel"

    rollover: (e) ->
        return false  unless @checkForActiveLoan()
        if @model.get("notExperiedRollover") and @model.get("notExperiedRollover").PaidPaymentAmount > 0
            EzBob.ShowMessage("Rollover is partially paid. Cannot be edited.")
            return false

        model =
            schedule: @model.get("details").Schedule
            rollover: @model.get("details").Rollovers
            configValues: @model.get("configValues")
            notExperiedRollover: @model.get("notExperiedRollover")
            loanId: @model.loanid

        @rolloverView = new EzBob.Underwriter.RolloverView(model: model)
        EzBob.App.jqmodal.show @rolloverView
        @rolloverView.on "addRollover", @addRollover, this
        @rolloverView.on "removeRollover", @removeRollover, this

    removeRollover: (roloverId) ->
        that = this
        BlockUi "on"
        $.post(window.gRootPath + "Underwriter/LoanHistory/RemoveRollover", roloverId).success((request) ->
            if request.success is false
                EzBob.ShowMessage request.error, "Something went wrong"
                return
            EzBob.ShowMessage "Rollover succesfully removed"
            that.model.fetch()
        ).done ->
            EzBob.App.jqmodal.hideModal(that.rolloverView)
            BlockUi "off"


    addRollover: (model) ->
        that = this
        BlockUi "on"
        $.post(window.gRootPath + "Underwriter/LoanHistory/AddRollover", model).success((request) ->
            if request.success is false
                EzBob.ShowMessage request.error, "Something went wrong"
                return
            EzBob.ShowMessage "Rollover succesfully " + ((if SerializeArrayToEasyObject(model).isEditCurrent is "true" then "edited" else "added"))
            that.model.fetch()
            that.trigger "RolloverAdded"
        ).done ->
            EzBob.App.jqmodal.hideModal(that.rolloverView)
            BlockUi "off"


    makePayment: (e) ->
        #allow manual payments
        #return  unless @checkForActiveLoan()
        model = loanId: @options.loan.Id
        view = new EzBob.Underwriter.ManualPaymentView(model: new Backbone.Model(model))
        EzBob.App.jqmodal.show view
        view.on "addPayment", @addPayment, this

    addPayment: (data) ->
        that = this
        data += "&CustomerId=" + @model.id
        data += "&LoanId=" + @options.loan.Id
        BlockUi "on"
        $.post(window.gRootPath + "Underwriter/LoanHistory/ManualPayment", data).success((response)->
            if response.error
                EzBob.ShowMessage response.error, "Something went wrong", ->
            else
                EzBob.ShowMessage "Manual payment succesfully added", "", ->
                    that.model.fetch()
                    that.trigger "ManualPaymentAdded"
                    true
        ).done ->
            BlockUi "off"

    showDialogOptions: ->
        @loanOptionsModel = new EzBob.Underwriter.LoanOptionsModel()
        @loanOptionsModel.loanId = @model.loanid
        xhr = @loanOptionsModel.fetch()
        that = this
        xhr.done ->
            @optionsView = new EzBob.Underwriter.LoanOptionsView(model: that.loanOptionsModel)
            @optionsView.render()
            EzBob.App.jqmodal.show @optionsView

    checkForActiveLoan: ->
        if @options.loan.Status is "PaidOff"
            EzBob.ShowMessage "Loan is  paid off", "Info"
            return false
        true

    exportToPdf: (e) ->
        customerId = @model.id
        $el = $(e.currentTarget);
        $el.attr("href", window.gRootPath + "Underwriter/LoanHistory/ExportDetails?id="+customerId+"&loanid="+@options.loan.Id+"&isExcel=false" + "&wError=" + @$el.find('.filter-errors').is(':checked') );
    
    exportToExcel: (e) ->
        customerId = @model.id
        $el = $(e.currentTarget);
        $el.attr("href", window.gRootPath + "Underwriter/LoanHistory/ExportDetails?id="+customerId+"&loanid="+@options.loan.Id+"&isExcel=true" + "&wError=" + @$el.find('.filter-errors').is(':checked') );