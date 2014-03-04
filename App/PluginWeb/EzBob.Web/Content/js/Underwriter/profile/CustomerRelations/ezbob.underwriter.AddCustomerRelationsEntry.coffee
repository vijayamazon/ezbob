root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.AddCustomerRelationsEntry extends EzBob.BoundItemView
    template: '#add-customer-relations-entry-template'

    events:
        'keyup textarea': 'commentKeyup'
    # end of events

    jqoptions: ->
        modal: true
        resizable: false
        title: 'CRM'
        position: 'center'
        draggable: false
        dialogClass: 'customer-relations-popup'
        width: 600
    # end of jqoptions

    initialize: (options) ->
        @model = new Backbone.Model(actions: options.actions, statuses: options.statuses)
        @mainTab = options.mainTab
        @onsave = options.onsave
        @onbeforesave = options.onbeforesave
        @customerId = if @mainTab then @mainTab.model.customerId else options.customerId
        @url = options.url or window.gRootPath + 'Underwriter/CustomerRelations/SaveEntry/'
        super()
    # end of initialize

    render: ->
        super()
        @$el.find('#Action').prop('selectedIndex',1)
        @
    # end of render

    commentKeyup: (el) ->
        $(el.target).val($(el.target).val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000))
    # end of commentKeyup

    onSave: ->
        return false if not $('#Incoming_I')[0].checked and not $('#Incoming_O')[0].checked
        return false if $('#Status')[0].selectedIndex is 0
        return false if $('#Action')[0].selectedIndex is 0

        BlockUi()

        opts =
            isIncoming: $('#Incoming_I')[0].checked,
            action: $('#Action')[0].value,
            status: $('#Status')[0].value,
            comment: $('#Comment').val(),
            customerId: @customerId,

        if @onbeforesave
            @onbeforesave opts

        xhr = $.post @url, opts

        xhr.done (r) =>
            UnBlockUi()

            if r.success
                if @mainTab
                    @mainTab.model.fetch()
                else if @onsave
                    @onsave()
            else
                if r.error?
                    EzBob.ShowMessage(r.error, 'Error')

            @close()

        xhr.complete => UnBlockUi()

        false
    #end of onSave
# end of class EzBob.Underwriter.AddCustomerRelationsEntry
