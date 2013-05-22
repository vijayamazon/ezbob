root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}


class EzBob.Profile.ApplyForLoanView extends Backbone.Marionette.ItemView
  template: "#apply-forloan-template"

  initialize: (options) ->
    @customer = options.customer

    if @customer.get("CreditSum") < EzBob.Config.XMinLoan
      @trigger "back"
      document.location.href = "#"
      return

    @fixed = @customer.get('IsLoanDetailsFixed')
    @isLoanTypeSelectionAllowed = @customer.get('IsLoanTypeSelectionAllowed')

    @currentLoanTypeID = @customer.get('LastApprovedLoanTypeID')
    @currentRepaymentPeriod = @customer.get('LastApprovedRepaymentPeriod')

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
    'click .plan': 'loanTypeChanged'

  ui:
    submit: ".submit"
    agreement: ".agreement"
    loanAmount: "input[name='loanAmount']"

  loanTypeChanged: (e) =>
    if e.target.tagName.toLowerCase() is 'a'
        return

    aryID = $(e.target).closest('.plan').attr('id').split '-'

    newLoanTypeID = aryID[2]
    newRepaymentPeriod = aryID[3]

    if newLoanTypeID == @currentLoanTypeID and newRepaymentPeriod == @currentRepaymentPeriod
      return

    @currentLoanTypeID = newLoanTypeID
    @currentRepaymentPeriod = newRepaymentPeriod

    @setCurrentlyActiveLoanType()

    @neededCashChanged true

  setCurrentlyActiveLoanType: =>
    @$('div.currently-active').removeClass('currently-active')
    sCurrentlyActive = @currentLoanTypeID + '-' + @currentRepaymentPeriod
    @$('#loan-type-' + sCurrentlyActive).addClass('currently-active')

  showSubmit: ->
    readPreAgreement = $(".preAgreementTermsRead").is(":checked")
    readAgreement = $(".agreementTermsRead").is(":checked")
    read = (readAgreement is true and readPreAgreement is true)
    @model.set "agree", read
    @$el.find(".submit").toggleClass "disabled", not read
    @$el.find("#getChashContinueBtn").toggleClass "disabled", not read

  loanAmountChanged: (e) ->
    amount = @ui.loanAmount.autoNumericGet()
    @model.set "neededCash", parseInt(amount, 10)
    @model.set "loanType", @currentLoanTypeID
    @model.set "repaymentPeriod", @currentRepaymentPeriod

  recalculateSchedule: (args) ->
    console.log 'recalculateSchedule', args
    val = args.value
    unless args.reloadSelectedOnly is true
      $.getJSON("#{window.gRootPath}Customer/Schedule/CalculateAll?amount=#{parseInt(val)}").done (data) =>
        for loanKey, offer of data
          $('#loan-type-' + loanKey + ' .Interest').text EzBob.formatPounds offer.TotalInterest
          $('#loan-type-' + loanKey + ' .Total').text EzBob.formatPounds offer.Total

    BlockUi "on", @$el.find('#block-loan-schedule')
    BlockUi "on", @$el.find('#block-agreement')
    sMoreParams = '&loanType=' + @currentLoanTypeID + '&repaymentPeriod=' + @currentRepaymentPeriod
    $.getJSON("#{window.gRootPath}Customer/Schedule/Calculate?amount=#{parseInt(val)}" + sMoreParams).done (data) =>
      @renderSchedule data
      BlockUi "off", @$el.find('#block-loan-schedule')
      BlockUi "off", @$el.find('#block-agreement')

  renderSchedule: (schedule) ->
    @lastPaymentDate = moment(schedule.Schedule[schedule.Schedule.length - 1].Date)
    scheduleView = new EzBob.LoanScheduleView(
      el: @$el.find(".loan-schedule")
      schedule: schedule
      isShowGift: true
      isShowExportBlock:false
    )
    scheduleView.render()
    @createAgreementView schedule.Agreement

  neededCashChanged: (reloadSelectedOnly) ->
    value = @model.get("neededCash")
    @ui.submit.attr "href", @model.get("url")
    @recalculateThrottled value: value, reloadSelectedOnly: reloadSelectedOnly
    @ui.loanAmount.autoNumericSet value
    @$el.find("#loan-slider").slider "value", value

  onRender: ->
    if @fixed
      @$(".cash-question").hide()

    if @isLoanTypeSelectionAllowed in [ 1, '1' ]
      @$('#loan-type-selector').show()
      @setCurrentlyActiveLoanType()

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
      @model.set "loanType", @currentLoanTypeID
      @model.set "repaymentPeriod", @currentRepaymentPeriod

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
    @$el.find('input[name=loanAmount]').autoNumeric EzBob.moneyFormatNoDecimals
    @neededCashChanged()

    @$el.find("img[rel]").setPopover 'right'
    @$el.find("li[rel]").setPopover 'left'

    this

  refreshTimer: ->
    @$el.find(".offerValidFor").text @customer.offerValidFormatted()

  submit: (e) ->
    e.preventDefault()
    creditSum = @model.get("neededCash")
    max = @model.get("maxCash")
    min = @model.get("minCash")
    @model.set "neededCash", creditSum
    @model.set "loanType", @currentLoanTypeID
    @model.set "repaymentPeriod", @currentRepaymentPeriod
    return false  if creditSum > max or creditSum < min
    return false  if not $(".preAgreementTermsRead").is(":checked") or not $(".agreementTermsRead").is(":checked")
    @trigger ("submit")
    return false

  getCurrentViewId: ->
    current = @$el.find("li.active a").attr("page-name")
    current

  print: ->
    printElement @getCurrentViewId()
    false

  download: ->
    amount = parseInt @model.get("neededCash"), 10
    loanType = @currentLoanTypeID
    repaymentPeriod = @currentRepaymentPeriod
    view = @getCurrentViewId()
    location.href = "#{window.gRootPath}Customer/Agreement/Download?amount=#{amount}&viewName=#{view}&loanType=#{loanType}&repaymentPeriod=#{repaymentPeriod}"
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