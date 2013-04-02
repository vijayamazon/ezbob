root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.ApplyForLoanModel extends Backbone.Model
  defaults:
    neededCash: 100
    maxCash: 15000
    minCash: EzBob.Config.MinLoan
    agree: false
    agreement: false
    CreditSum: 0
    OfferValid: 0
    OfferValidMintes: 0

  validate: (attrs) ->
    unless typeof attrs.neededCash is "undefined"
      val = attrs.neededCash
      attrs.neededCash = @get("minCash")  if isNaN(val)
      attrs.neededCash = @get("maxCash")  if val > @get("maxCash")
      attrs.neededCash = @get("minCash")  if val < @get("minCash")
    false

  initialize: ->
    @set
      neededCash: @get("maxCash")
      minCash: (if @get("maxCash") > EzBob.Config.MinLoan then EzBob.Config.MinLoan else EzBob.Config.XMinLoan)

class EzBob.Profile.ApplyForLoanView extends Backbone.Marionette.ItemView
  template: "#apply-forloan-template"

  initialize: (options) ->
    @customer = options.customer

    if @customer.get("CreditSum") < EzBob.Config.XMinLoan
      @trigger "back"
      document.location.href = "#"
      return

    @fixed = @customer.get('IsLoanDetailsFixed')

    @model = new EzBob.Profile.ApplyForLoanModel(
      maxCash: @customer.get("CreditSum")
      OfferStart: @customer.get("OfferStart")
    )

    @recalculateThrottled = _.debounce @recalculateSchedule, 250
    @timerId = setInterval _.bind(@refreshTimer, this), 1000
    @model.set "CreditSum": @customer.get("CreditSum"), "OfferValid": @customer.offerValidFormatted()

    if(@fixed)
      #@neededCashChanged()
      return

    @model.on "change:neededCash", @neededCashChanged, this

  events:
    "click .submit": "submit"
    "change input[name='loanAmount']": "loanAmountChanged"
    "change .preAgreementTermsRead": "showSubmit"
    "change .agreementTermsRead": "showSubmit"
    "click .download": "download"
    "click .print": "print"

  ui:
    submit: ".submit"
    agreement: ".agreement"
    loanAmount: "input[name='loanAmount']"

  showSubmit: ->
    readPreAgreement = $(".preAgreementTermsRead").is(":checked")
    readAgreement = $(".agreementTermsRead").is(":checked")
    read = (readAgreement is true and readPreAgreement is true)
    @model.set "agree", read
    @$el.find(".submit").toggleClass "disabled", not read

  loanAmountChanged: (e) ->
    amount = @ui.loanAmount.autoNumericGet()
    @model.set "neededCash", parseInt(amount, 10)

  recalculateSchedule: (val) ->
    BlockUi "on", @$el.find('#block-loan-schedule')
    BlockUi "on", @$el.find('#block-agreement')
    $.getJSON("#{window.gRootPath}Customer/Schedule/Calculate?amount=#{parseInt(val)}").done (data) =>
      @renderSchedule data
      BlockUi "off", @$el.find('#block-loan-schedule')
      BlockUi "off", @$el.find('#block-agreement')

  renderSchedule: (schedule) ->
    @lastPaymentDate = moment(schedule.schedule[schedule.schedule.length - 1].Date)
    scheduleView = new EzBob.LoanScheduleView(
      el: @$el.find(".loan-schedule")
      schedule: schedule
      isShowGift: true
    )
    scheduleView.render()
    @createAgreementView schedule.agreement

  neededCashChanged: ->
    value = @model.get("neededCash")
    @ui.submit.attr "href", "GetCash/GetTransactionId?loan_amount=" + value
    @recalculateThrottled value
    @ui.loanAmount.autoNumericSet value
    @$el.find("#loan-slider").slider "value", value

  onRender: ->

    if @fixed
      @$(".cash-question").hide()

    updateSlider = (event, ui) =>
      percent = (ui.value - min) / (max - min) * 100
      slider = @$el.find(".ui-slider")
      slider.css "background", "-webkit-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) #{percent}%,rgba(201,201,201,1) #{percent}%,rgba(229,229,229,1) 100%)"
      slider.css "background", "-moz-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) #{percent}%,rgba(201,201,201,1) #{percent}%,rgba(229,229,229,1) 100%)"
      slider.css "background", "-o-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) #{percent}%,rgba(201,201,201,1) #{percent}%,rgba(229,229,229,1) 100%)"
      slider.css "background", "-ms-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) #{percent}%,rgba(201,201,201,1) #{percent}%,rgba(229,229,229,1) 100%)"
      slider.css "-pie-background", "linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) #{percent}%,rgba(201,201,201,1) #{percent}%,rgba(229,229,229,1) 100%)"
      return  if ui.value is @model.get("neededCash")
      @model.set "neededCash", ui.value

    max = @model.get("maxCash")
    min = @model.get("minCash")
    sliderOptions =
      max: max
      min: min
      value: @model.get("neededCash")
      step: EzBob.Config.GetCashSliderStep
      slide: updateSlider
      change: updateSlider

    @$el.find("#loan-slider").slider sliderOptions
    
    #this.$el.find('input[name="loanAmount"]').numericOnly();
    @$el.find("input[name=\"loanAmount\"]").moneyFormat()
    @neededCashChanged()

    @$el.find("img[rel]").setPopover 'right'
    @$el.find("li[rel]").setPopover 'left'

    this

  refreshTimer: ->
    @$el.find(".offerValidFor").text @customer.offerValidFormatted()

  submit: ->
    creditSum = @model.get("neededCash")
    max = @model.get("maxCash")
    min = @model.get("minCash")
    @model.set "neededCash", creditSum
    return false  if creditSum > max or creditSum < min
    return false  if not $(".preAgreementTermsRead").is(":checked") or not $(".agreementTermsRead").is(":checked")

    view = new EzBob.Profile.PayPointCardSelectView( model: @customer, date: @lastPaymentDate )
    if view.cards.length > 0
      view.on 'select', (cardId) =>
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Customer/GetCash/Now", {cardId: cardId, amount: creditSum}
        xhr.done (data) =>
          document.location.href = data.url;
      
      view.on 'existing', => @_submit()

      EzBob.App.modal.show view
      return false
    else
      BlockUi "off"
  
  #        var model = new Backbone.Model({
  #            amount: creditSum,
  #            card_no: this.customer.get('BankAccountNumber').substring(4, 8),
  #            name: this.customer.get('CustomerPersonalInfo').FirstName,
  #            surname: this.customer.get('CustomerPersonalInfo').Surname,
  #            total: this.total
  #        });
  #        
  #        var view = new EzBob.GetCashConfirmation({ model: model });
  #        view.on('modal:save', this._submit, this);
  #        EzBob.App.modal.show(view);
  #
  #        return false;
  #        
  _submit: ->
    BlockUi "on"
    document.location.href = @ui.submit.attr("href")

  getCurrentViewId: ->
    current = @$el.find("li.active a").attr("page-name")
    current

  print: ->
    printElement @getCurrentViewId()
    false

  download: ->
    amount = parseInt @model.get("neededCash"), 10
    view = @getCurrentViewId()
    location.href = "#{window.gRootPath}Customer/Agreement/Download?amount=#{amount}&viewName=#{view}"
    false

  createAgreementView: (agreementdata) ->
    bussinessType = (@customer.get "CustomerPersonalInfo").TypeOfBusiness
    #consumer, soletrader, pship up to 3
    isConsumer = bussinessType is 0 or bussinessType is 4 or bussinessType is 2
    view = if isConsumer then new EzBob.Profile.ConsumersAgreementView(el: @ui.agreement) else new EzBob.Profile.CompaniesAgreementView(el: @ui.agreement)
    view.render agreementdata
    @showSubmit()

  close: ->
    clearInterval @timerId
    @model.off()
    super()

EzBob.Profile.ThankyouLoan = Backbone.View.extend(
  initialize: ->
    @template = _.template($("#thankyouloan-template").html())

  events:
    "click a": "close"

  render: ->
    @$el.html @template()

  close: ->
    @trigger "close"
    false
)

EzBob.GetCashConfirmation = Backbone.Marionette.ItemView.extend(
  template: "#apply-forloan-confirmation-template"
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