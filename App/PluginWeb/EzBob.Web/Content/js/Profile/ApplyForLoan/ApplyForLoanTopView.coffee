root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.ApplyForLoanTopViewModel extends Backbone.Model
    defaults:
        state: "apply"

# a top view that manages process of choosing loan amount
# adding a bank account and displaying confirmation screen
class EzBob.Profile.ApplyForLoanTopView extends Backbone.Marionette.ItemView

    initialize: (options)->
        @template = _.template($("#apply-forloan-top-template").html())

        #@region = new Backbone.Marionette.Region el: @$el.find('.apply-for-loan-div')
        @customer = options.customer
        @applyForLoanViewModel = new EzBob.Profile.ApplyForLoanModel(
          maxCash: @customer.get("CreditSum")
          OfferStart: @customer.get("OfferStart")
          loanType: @customer.get("LastApprovedLoanTypeID")
          repaymentPeriod: @customer.get("LastApprovedRepaymentPeriod")
          isLoanSourceEU: @customer.get("IsLastApprovedLoanSourceEu")
          isCurrentCashRequestFromQuickOffer: @customer.get("IsCurrentCashRequestFromQuickOffer")
        )

        @states = 
            apply: @createApplyForLoanView
            bank: @createAddBankAccountView

        @model.on "change", @render, this

    render: ->
        @$el.html @template()
        view = @states[@model.get("state")]()
        region = new Backbone.Marionette.Region el: @$el.find('.apply-for-loan-div')
        region.show view
        #@createApplyForLoanView()
        false 

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

    amountSelected: () ->
        form = @$el.find('form')

        pi = @customer.get 'CustomerPersonalInfo'
        
        @$el.find('#signedName').attr('maxlength', pi.Fullname.length + 10)

        enabled = EzBob.Validation.checkForm EzBob.validateLoanLegalForm(form, [ pi.FirstName, pi.MiddleInitial, pi.Surname ])

        return unless enabled

        data = form.serialize()

        BlockUi "on"

        xhr = $.post "#{window.gRootPath}Customer/GetCash/LoanLegalSigned", data

        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
                 return

            if not @customer.get("bankAccountAdded")
                @model.set "state", "bank"
                return

            @submit()
        xhr.always ->
            BlockUi "off"

    submit: ->
        view = new EzBob.Profile.PayPointCardSelectView( model: @customer, date: @lastPaymentDate )
        if view.cards.length > 0
            view.on 'select', (cardId) =>
                BlockUi "on"
                xhr = $.post "#{window.gRootPath}Customer/GetCash/Now", {cardId: cardId, amount: @applyForLoanViewModel.get("neededCash")}
                xhr.done (data) =>
                    if data.error != undefined
                        EzBob.ShowMessage data.error, "Error occured"
                    else 
                        document.location.href = data.url;
                xhr.complete ->
                    BlockUi "off"
            view.on 'existing', => @_submit()
            view.on 'cancel', => @model.set("state", "apply")
            EzBob.App.modal.show view
            return false
        else
            @_submit()
        return false

    _submit: ->
        BlockUi "on"
        @applyForLoanViewModel.buildUrl()
        document.location.href = @applyForLoanViewModel.get('url')

