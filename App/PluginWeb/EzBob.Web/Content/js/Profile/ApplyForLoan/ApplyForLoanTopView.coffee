root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.ApplyForLoanTopViewModel extends Backbone.Model
    defaults:
        state: "apply"

# a top view that manages process of choosing loan amount
# adding a bank account and displaying confirmation screen
class EzBob.Profile.ApplyForLoanTopView extends Backbone.Marionette.View
    initialize: (options)->
        @region = new Backbone.Marionette.Region el: @el
        @customer = options.customer

        @applyForLoanViewModel = new EzBob.Profile.ApplyForLoanModel(
          maxCash: @customer.get("CreditSum")
          OfferStart: @customer.get("OfferStart")
        )

        @states = 
            apply: @createApplyForLoanView
            bank: @createAddBankAccountView

        @model.on "change", @render, this

    render: ->
        view = @states[@model.get("state")]()
        @region.show(view)

    createApplyForLoanView: =>
        view = new EzBob.Profile.ApplyForLoanView(model: @applyForLoanViewModel, customer: @customer)
        #view.on "back", -> @trigger("back")
        view.on "submit", @amountSelected, this
        return view

    createAddBankAccountView: =>
        view = new EzBob.BankAccountInfoView( model: @customer )
        view.on "back", => @model.set("state", "apply")
        view.on "completed", => @submit()
        return view

    amountSelected: ->
        console.log "amount #{@applyForLoanViewModel.get('neededCash')} selected url is #{@applyForLoanViewModel.get('url')}"

        if not @customer.get("bankAccountAdded")
            @model.set "state", "bank"
            return

        @submit()

    submit: ->
        view = new EzBob.Profile.PayPointCardSelectView( model: @customer, date: @lastPaymentDate )
        if view.cards.length > 0
            view.on 'select', (cardId) =>
                BlockUi "on"
                xhr = $.post "#{window.gRootPath}Customer/GetCash/Now", {cardId: cardId, amount: creditSum}
                xhr.done (data) =>
                    document.location.href = data.url;
      
            view.on 'existing', => @_submit()

            view.on 'cancel', => @model.set("state", "apply")

            EzBob.App.modal.show view
            return false

        return false

    _submit: ->
        BlockUi "on"
        document.location.href = @applyForLoanViewModel.get('url')

