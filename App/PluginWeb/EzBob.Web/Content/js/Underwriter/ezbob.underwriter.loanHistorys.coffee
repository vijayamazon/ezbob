root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.LoanHistoryModel extends Backbone.Model
    idAttribute: "Id"
    url: -> "#{window.gRootPath}Underwriter/LoanHistory/Index/#{@customerId}"

class EzBob.Underwriter.LoanHistoryView extends Backbone.Marionette.View
    initialize: ->
        @template = _.template($("#loanhistory-template").html())
        @templateView = _.template($("#loanhistory-view-template").html())
        @bindTo @model, "reset fetch change", @render, this

    events:
        "click tr.loans.tr-link"   : "rowClick"
        "click .export-to-exel"    : "exportExcel"
        "click .edit-loan"         : "editLoan"
        "click .show-schedule"     : "showSchedule"

    exportExcel: ->
        location.href =  "#{window.gRootPath}Underwriter/LoanHistory/ExportToExel?id=#{@model.customerId}"

    rowClick: (e) ->
        id = +e.currentTarget.getAttribute("data-id")
        return unless id?
        details = new EzBob.Underwriter.LoanHistoryDetailsModel()
        details.loanid = id
        loan = _.find(@model.get("loans"), (l) ->
            l.Id is id
        )
        details.id = @idCustomer

        detailsView = new EzBob.Underwriter.LoanDetailsView(model: details, loan: loan)
        detailsView.on "RolloverAdded",  @updateView, @
        detailsView.on "ManualPaymentAdded", @updateView, @
        details.fetch()

    updateView: ->
        @model.fetch()

    editLoan: (e) ->
        id = e.currentTarget.getAttribute("data-id")
        loan = new EzBob.LoanModel Id: id
        xhr = loan.fetch()
        xhr.done =>
            view = new EzBob.EditLoanView model:loan
            view.on "item:saved", @updateView, this
            EzBob.App.jqmodal.show view
        false

    render: ->
        @$el.html @templateView()
        @table = @$el.find("#loanhistory-table")
        viewModel = @model.toJSON()
        @table.html @template(viewModel)
        this

    showSchedule: (e) ->
        offerId = $(e.currentTarget).data('id')

        xhr = $.getJSON  "#{window.gRootPath}Underwriter/Schedule/Calculate/#{offerId}"

        xhr.done (data) =>
            view = new EzBob.LoanScheduleViewDlg schedule: data, isShowGift: false
            EzBob.App.jqmodal.show view
        false