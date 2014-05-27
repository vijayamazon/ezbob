root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.PaymentAccountsModel extends Backbone.Model
    urlRoot: ->
        "#{window.gRootPath}Underwriter/PaymentAccounts/Index/#{@customerId}"

    getCardById: (id) ->
        current = @get('CurrentBankAccount')
        return  current if parseInt(current.Id) is parseInt(id)
        for acc in @get('BankAccounts') when parseInt(acc.Id) is parseInt(id)
            return acc

class EzBob.Underwriter.PaymentAccountView extends Backbone.Marionette.ItemView
    template: "#payment-accounts-template"

    initialize: ->
        @bindTo @model, "change reset sync", @render, @
        window.paypointAdded = (amount = 5) =>
            EzBob.ShowMessage "Please deduct the #{amount} pounds from this card using manual payment", "Reminder"
            @model.fetch()

    serializeData: ->
        bankAccounts = @model.get("BankAccounts") || []
        current = @model.get("CurrentBankAccount")
        if current
            current.isDefault = true
            bankAccounts.push(current)
        
        bankAccounts = _.sortBy(bankAccounts, "BankAccount")

        return {
            bankAccounts: bankAccounts
            paymentAccounts: @model.toJSON()
            customerId: @model.customerId
        }

    ui:
        'allowSelection' : '.debitCardCustomerSelection'
    events:
        "click .bankAccounts tbody tr"   : "showBankAccount"
        "click .checkeBankAccount"       : "checkBanckAccount"
        "click .add-existing"            : "addExistingCard"
        "click .setDefault"              : "setDefault"
        "click .addNewDebitCard"         : "addNewDebitCard"
        "click .set-paypoint-default"    : "setPaypointDefault"

    onRender: ->
        #@$el.find('a[data-bug-type]').tooltip({title: 'Report bug'});
        @$el.find('.bankAccounts i[data-title]').tooltip({placement: "right"})
        
        @ui.allowSelection.bootstrapSwitch()
        @ui.allowSelection.bootstrapSwitch('setState', @model.get('CustomerDefaultCardSelectionAllowed'))
        @ui.allowSelection.on('switch-change', (event, state) =>
            @changeAllowSelection(event, state)
        )

    changeAllowSelection: (event, state) ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/PaymentAccounts/ChangeCustomerDefaultCardSelection", {customerId: @model.customerId, state: state.value }
        xhr.done =>
            @model.fetch()
        xhr.always ->
            BlockUi "off"

    setPaypointDefault: (e)->

        $el = ($ e.currentTarget)
        transactionId = $el.data "transactionid"

        card = _.find @model.get("PayPointCards"), (c) => c.TransactionId == transactionId

        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/PaymentAccounts/SetPaypointDefaultCard", {customerId: @model.customerId, transactionId: transactionId, cardNo: card.CardNo }
        xhr.complete ->
            BlockUi "off"
        xhr.done =>
            @model.fetch()

    showBankAccount: (e) ->
        return false if e.target.tagName == 'BUTTON'
        id = $(e.currentTarget).data 'card-id'
        @_showBankAccount(id)

    _showBankAccount: (cardId) ->
        card = @model.getCardById cardId
        if not card?.Bank?
            message = card?.StatusInformation or "Validation was not performed"
            EzBob.ShowMessage message, "Bank account check"
            return false
        view = new EzBob.Underwriter.BankAccountDetailsView model: new Backbone.Model(card)
        EzBob.App.jqmodal.show view
        false

    addNewDebitCard: ->
        view = new EzBob.Underwriter.AddBankAccount(customerId: @model.customerId)
        view.on 'saved', =>
            @model.fetch()
        EzBob.App.jqmodal.show view
        false

    setDefault: (e) ->
        id = $(e.currentTarget).parents('tr').data 'card-id'
        xhr = $.post "#{window.gRootPath}Underwriter/PaymentAccounts/SetDefaultCard", {customerId: @model.customerId, cardId: id}
        xhr.done =>
            @model.fetch()
        return

    checkBanckAccount: (e) ->
        id = $(e.currentTarget).parents('tr').data 'card-id'
        BlockUi 'On'
        
        xhr = $.ajax
            url: "#{window.gRootPath}Underwriter/PaymentAccounts/PerformCheckBankAccount"
            data: {id: @model.customerId, cardid: id}
            global: false
            type: 'POST'

        xhr.done  (r) =>
            if (r.error)
                @model.fetch()
                BlockUi 'Off'
                EzBob.ShowMessage(r.error, "Error")
                return
            xhr2 = @model.fetch()
            xhr2.done =>
                @_showBankAccount(id)
                BlockUi 'Off'

    addExistingCard: ->
        model = new EzBob.Underwriter.PayPointCardModel()
        view = new EzBob.Underwriter.AddPayPointCardView( model: model )
        view.on 'save', =>
            BlockUi "on"
            data = model.toJSON()
            data.customerId = @model.customerId
            xhr = $.post "#{window.gRootPath}Underwriter/PaymentAccounts/AddPayPointCard", data
            xhr.done =>
                BlockUi "off"
                @model.fetch()
        EzBob.App.jqmodal.show view
        return false
