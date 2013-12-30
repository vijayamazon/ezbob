root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.EditLoanView extends Backbone.Marionette.ItemView
    template: "#loan_editor_template"
    scheduleTemplate: _.template($("#loan_editor_schedule_template").html())

    initialize: ->
        @bindTo @model, "change sync", @renderRegions, this
        @editItemIndex = -1

    serializeData: ->
        data = @model.toJSON()

        e = new Error('dummy')
        stack = e.stack.replace(/^[^\(]+?[\n$]/gm, '').replace(/^\s+at\s+/gm, '').replace(/^Object.<anonymous>\s*\(/gm, '{anonymous}()@').split('\n')

        data.editItemIndex = @editItemIndex
        return data

    ui:
        scheduleEl    : ".editloan-schedule-region"
        freezeEl      : ".editloan-freeze-intervals-region"
        ok            : ".save"
        buttons       : ".buttons"

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
        "click .add-freeze-interval"    : "onAddFreezeInterval"
        "click .remove-freeze-interval" : "onRemoveFreezeInterval"

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
            @renderSchedule(@serializeData())

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
        @renderRegions()

    renderRegions: ->
        data = @serializeData()
        @renderSchedule(data)
        @renderFreeze(data)

    renderSchedule: (data) ->
        @ui.scheduleEl.html(@scheduleTemplate(data))
        @ui.ok.toggleClass "disabled", @model.get("HasErrors")

    renderFreeze: (data) ->
        @ui.freezeEl.html(_.template($("#loan_editor_freeze_intervals_template").html())(data))

    onAddFreezeInterval: ->
        sStart = @$el.find(".new-freeze-interval-start").val()
        sEnd   = @$el.find(".new-freeze-interval-end").val()
        nRate  = @$el.find(".new-freeze-interval-rate").val() / 100.0

        @$el.find('.new-freeze-interval-error').empty()

        if @validateFreezeIntervals sStart, sEnd
            @model.addFreezeInterval sStart, sEnd, nRate
        else
            @$el.find('.new-freeze-interval-error').text('New interval conflicts with one of existing intervals');

    onRemoveFreezeInterval: (evt) ->
        @model.removeFreezeInterval evt.currentTarget.getAttribute("data-id")

    validateFreezeIntervals: (sStartDate, sEndDate) ->
        oStart = moment.utc sStartDate
        oEnd = moment.utc sEndDate

        if oStart != null and oEnd != null and oStart > oEnd
            @$el.find(".new-freeze-interval-start").val sEndDate
            @$el.find(".new-freeze-interval-end").val sStartDate
            tmp = oEnd
            oEnd = oStart
            oStart = tmp

        bConflict = false

        _.each @model.get('InterestFreeze'), (item, idx) =>
            if bConflict
                return

            ary = item.split '|'

            if ary[4] != ''
                return

            oLeft = moment.utc ary[0]
            oRight = moment.utc ary[1]

            # interval intersection rule e.g. here: http://world.std.com/~swmcd/steven/tech/interval.html

            bFirst = @cmpDates oStart, oRight
            bSecond = @cmpDates oLeft, oEnd

            bConflict = bFirst and bSecond

            # fmt = (o) -> return if o then o.format 'DD/MM/YYYY' else ''
            # console.log('[', fmt(oLeft), '-', fmt(oRight), '] [', fmt(oStart), '-', fmt(oEnd), '] intersect:', bFirst and bSecond)
        # end of foreach handler

        return not bConflict
    # end of validateFreezeIntervals

    # returns result of comparison a <= b
    # if a is null it is negative infinity
    # if b is null it is positive infinity
    cmpDates: (a, b) ->
        if a == null or b == null
            return true

        return a <= b
    # end of cmpDates

    onClose: ->
        @editRegion.close()

    jqoptions: ->
        width: '80%'
        modal: true
        title: 'Edit Loan Details'
        resizable: true