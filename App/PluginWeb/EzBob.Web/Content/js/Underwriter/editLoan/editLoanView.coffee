root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.EditLoanView extends Backbone.Marionette.ItemView
    template: "#loan_editor_template"
    scheduleTemplate: _.template($("#loan_editor_schedule_template").html())

    initialize: ->
        @bindTo @model, "change sync", @renderSchedule, this
        @editItemIndex = -1

    serializeData: ->
        data = @model.toJSON()
        data.editItemIndex = @editItemIndex
        return data

    ui:
        scheduleEl  : ".editloan-schedule-region"
        ok          : ".save"
        buttons     : ".buttons"

    editors:
        "Installment" : EzBob.InstallmentEditor
        "Fee" : EzBob.FeeEditor

    events:
        "click .edit-schedule-item"     : "editScheduleItem"
        "click .remove-schedule-item"   : "removeScheduleItem"
        "click .add-installment"        : "addInstallment"
        "click .add-fee"                : "addFee"
        "click .save"                   : "onOk"
        "click .cancel"                 : "onCancel"

    addInstallment: ->
        date = new Date @model.get("Items").last().get("Date")
        date.setMonth date.getMonth()+1
        installment = new EzBob.Installment({
            "Editable"                  : true
            "Deletable"                 : true
            "Editor"                    : "Installment"
            "Principal"                 : 0
            "Balance"                   : 0
            "BalanceBeforeRepayment"    : 0
            "Interest"                  : 0
            "InterestRate"              : @model.get("InterestRate")
            "Fees"                      : 0
            "Total"                     : 0
            "Status"                    : "StillToPay"
            "Type"                      : "Installment"
            "IsAdding"                  : true
            "Date"                      : date
        })
        editor = @editors["Installment"]
        view = new editor(model: installment, loan: @model)

        add = () ->
            @model.addInstallment(installment)

        view.on "apply", add, this
        
        @showEditView(view)

    addFee: ->
        fee = new EzBob.Installment({
            "Editable"                  : true
            "Deletable"                 : true
            "Editor"                    : "Fee"
            "Principal"                 : 0
            "Balance"                   : 0
            "BalanceBeforeRepayment"    : 0
            "Interest"                  : 0
            "InterestRate"              : @model.get("InterestRate")
            "Fees"                      : 0
            "Total"                     : 0
            "Type"                      : "Fee"
            "IsAdding"                  : true
            "Date"                      : @model.get("Items").last().get("Date")
            })

        editor = @editors["Fee"]
        view = new editor(model: fee, loan: @model)

        add = () ->
            @model.addFee(fee)

        view.on "apply", add, this

        @showEditView(view)

    showEditView: (view) ->
        view.on "close", =>
            @ui.buttons.show()
        @editRegion.show(view)
        @ui.buttons.hide()

    removeScheduleItem: (e) ->
        id = e.currentTarget.getAttribute("data-id")
        
        ok = () =>
            @model.removeItem id

        EzBob.ShowMessage "Confirm deleting installment", "Delete installment", ok, "Ok", null, "Cancel"

    editScheduleItem: (e) ->
        id = e.currentTarget.getAttribute("data-id")
        row = $(e.currentTarget).parents('tr')
        row.addClass("editing")
        item = @model.get("Items").at(id)
        @editItemIndex = id
        editor = @editors[item.get("Editor")]
        view = new editor(model: item, loan: @model)
        view.on "apply", @recalculate, this

        closed = () ->
            row.removeClass("editing")
            @editItemIndex = -1
            @renderSchedule()

        view.on "close", closed, this
        @showEditView(view)
    
    recalculate: ->
        @model.recalculate()

    onOk: ->
        return if @ui.ok.hasClass('disabled')
        xhr = @model.save()
        xhr.done =>
            @trigger "item:saved"
            @close()

    onCancel: ->
        @close()

    onRender: ->
        @editRegion = new Backbone.Marionette.Region el: @$(".editloan-item-editor-region")
        @renderSchedule()

    renderSchedule: ->
        @ui.scheduleEl.html(@scheduleTemplate(@serializeData()))
        @ui.ok.toggleClass "disabled", @model.get("HasErrors")

    onClose: ->
        @editRegion.close()

    jqoptions: ->
        width: 1000
        modal: true
        title: 'Edit Loan Details'
        resizable: false