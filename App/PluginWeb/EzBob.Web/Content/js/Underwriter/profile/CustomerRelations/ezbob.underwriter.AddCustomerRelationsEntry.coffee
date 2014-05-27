root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.AddCustomerRelationsEntry extends EzBob.BoundItemView
    template: '#add-customer-relations-entry-template'

    events:
        'keyup #Comment': 'commentKeyup'
    # end of events

    jqoptions: ->
        modal: true
        resizable: true
        title: 'CRM - add entry'
        position: 'center'
        draggable: true
        dialogClass: 'customer-relations-popup'
        width: 600
    # end of jqoptions

    initialize: (options) ->
        @onsave = options.onsave
        @onbeforesave = options.onbeforesave
        @customerId = this.model.customerId
        @url = window.gRootPath + 'Underwriter/CustomerRelations/SaveEntry/'
        super()
    # end of initialize

    onRender: ->
        @ui.Action.prop('selectedIndex',1)
    # end of render

    serializeData: ->
        data =
            actions: EzBob.CrmActions
            statuses: EzBob.CrmStatuses
            ranks: EzBob.CrmRanks
        return data

    commentKeyup: (el) ->
        @ui.Comment.val(@ui.Comment.val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000))
    # end of commentKeyup
    ui:
        "Incoming" : "#Incoming_I"
        "Status" : "#Status"
        "Action" : "#Action"
        "Rank" : "#Rank"
        "Comment" : "#Comment"

    onSave: ->
        return false if @ui.Status[0].selectedIndex is 0
        return false if @ui.Action[0].selectedIndex is 0
        return false if @ui.Rank[0].selectedIndex is 0

        BlockUi()

        opts =
            isIncoming: @ui.Incoming[0].checked,
            action: @ui.Action[0].value,
            status: @ui.Status[0].value,
            rank: @ui.Rank[0].value,
            comment: @ui.Comment.val(),
            customerId: @customerId,
        
        if @onbeforesave
            @onbeforesave opts

        xhr = $.post @url, opts

        xhr.done (r) =>
            if r.success
                @model.fetch()
            else
                if r.error
                    EzBob.ShowMessage(r.error, 'Error')

            @close()

        xhr.always => UnBlockUi()

        false
    #end of onSave
# end of class EzBob.Underwriter.AddCustomerRelationsEntry
