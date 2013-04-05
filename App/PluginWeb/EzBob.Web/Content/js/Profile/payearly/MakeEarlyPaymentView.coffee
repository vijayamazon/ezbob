root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}


class EzBob.Profile.MakeEarlyPayment extends Backbone.Marionette.ItemView
  template: "#payEaryly-template"
  initialize: (options) ->
    @infoPage = _.template($("#infoPageTemplate").html())
    @customerModel = options.customerModel
    @bindTo @customerModel, "change:LateLoans change:TotalBalance change:NextPayment change:ActiveLoans change:hasLateLoans", @render, this
    @loans = @customerModel.get("Loans")
    @model = new EzBob.Profile.MakeEarlyPaymentModel(customer: @customerModel)

    firstLate = _.find(@loans.toJSON(), (val,i) -> return val.Status is "Late")
    
    currentLoanId = undefined

    if @model.get('rollovers').length>0
        currentLoanId = @model.get('rollovers').toJSON()[0].LoanId
    else
        if firstLate 
            currentLoanId = firstLate.Id
        else
            if options.loanId
                currentLoanId = options.loanId
    
    @model.set (loan: @loans.get(currentLoanId))  if currentLoanId

    @bindTo @model, "change", @render, @
    @

  serializeData: ->
    data = @model.toJSON()
    data.hasLateLoans = @customerModel.get("hasLateLoans")
    data

  onRender: ->
    @$el.find("li[rel]").setPopover('left')
    @

  events:
    "click .submit": "submit"
    "change input[name='paymentAmount']": "paymentAmountChanged"
    "change input[name='rolloverAmount']": "rolloverAmountChanged"
    "change input[name='loanPaymentType']": "loanPaymentTypeChanged"
    "change input[name='rolloverPaymentType']": "rolloverPaymentTypeChanged"
    "change input[name='defaultCard']": "defaultCardChanged"
    "change select": "loanChanged"
    "click .back": "back"
    "click .back-to-profile": "backToProfile"
    "change input[name='paymentType']": "paymentTypeChanged"

  ui:
    submit: ".submit"

  defaultCardChanged: ->
    @model.set "defaultCard", not @model.get("defaultCard")

  submit: ->
    return false if @ui.submit.hasClass("disabled")
    
    if @model.get("defaultCard")
      @payFast()
      return false

    view = new EzBob.Profile.PayPointCardSelectView( model: @customerModel, date: moment() )

    return unless view.hasCards()

    view.on 'select', (cardId) =>
      @payFast cardId

    view.on 'existing', => 
      document.location.href = @ui.submit.attr("href")

    EzBob.App.modal.show view
    return false

  payFast: (cardId = -1) ->
      @ui.submit.addClass "disabled"
      data =
        amount: parseFloat( @model.get("amount") )
        type: @model.get("paymentType")
        paymentType: (if @model.get("paymentType") != "rollover" then @model.get("loanPaymentType") else @model.get("rolloverPaymentType") )
        loanId: @model.get("loan").id
        cardId: cardId
        rolloverId: @model.get("currentRollover") && @model.get("currentRollover").Id
      
      BlockUi "on"  
      $.post(window.gRootPath + "Customer/Paypoint/PayFast", data).done((res) =>
        if res.error
          EzBob.App.trigger "error", res.error
          @back()
          return
        loan = @model.get("loan")
        hadRollover = @model.get("currentRollover")

        @$el.html @infoPage(
          amount: res.PaymentAmount
          card_no: @customerModel.get("CreditCardNo")
          email: @customerModel.get("Email")
          name: @customerModel.get("CustomerPersonalInfo").FirstName
          surname: @customerModel.get("CustomerPersonalInfo").Surname
          refnum: (if loan then loan.get("RefNumber") else "")
          transRefnums: res.TransactionRefNumbersFormatted
          saved: res.Saved
          savedPounds: res.SavedPounds
          hasLateLoans: @customerModel.get("hasLateLoans")
          isRolloverPaid: res.RolloverWasPaid
        )
        EzBob.App.trigger "clear"
      ).complete =>
        @ui.submit.removeClass "disabled"
        BlockUi "off"  

  backToProfile: ->
    @customerModel.fetch()
    @trigger "submit"
    false

  paymentAmountChanged: ->
    amount = @$el.find("[name='paymentAmount']").autoNumericGet()
    maxAmount = @model.get("loan").get("TotalEarlyPayment")
    minAmount = if @model.get("currentRollover") is null then 30 else @model.get("currentRollover").RolloverPayValue
    if maxAmount < minAmount
        amount = maxAmount
    else if amount < minAmount
        amount = minAmount
    else if amount > maxAmount
        amount = maxAmount

    @model.set "amount", parseFloat(amount)
    @render()

  rolloverAmountChanged: ->
    amount = @$el.find("[name='rolloverAmount']").autoNumericGet()
    maxAmount = @model.get("total")
    minAmount = @model.get("currentRollover").RolloverPayValue
            
    amount = minAmount  if amount < minAmount 
    amount = maxAmount if amount > maxAmount

    @model.set "amount", parseFloat(amount)
    @render()

  paymentTypeChanged: ->
    type = @$el.find("input[name='paymentType']:checked").val()
    @model.set paymentType: type
    @loanChanged()

  loanPaymentTypeChanged: ->
    type = @$el.find("input[name='loanPaymentType']:checked").val()
    @model.set( {paymentType: "loan"}, {silent: true} ) unless @model.get("paymentType")=="loan"  
    @model.set loanPaymentType: type
    

  rolloverPaymentTypeChanged:->
    type = @$el.find("input[name='rolloverPaymentType']:checked").val()
    @model.set rolloverPaymentType: type

  loanChanged: ->
    loanId = $("select:NOT(:disabled):visible").val()
    if loanId != undefined
        loan = @customerModel.get("Loans").get(loanId)
        @model.set loan: loan

  back: ->
    @trigger "back"
    false

EzBob.PayEarlyConfirmation = Backbone.Marionette.ItemView.extend(
  template: "#pay-early-confirmation"
  events:
    "click a.cancel": "btnClose"
    "click a.save": "btnSave"

  btnClose: ->
    @close()
    false

  btnSave: ->
    @trigger "modal:save"
    @onOk()
    @close()
    false

  onOk: ->
)