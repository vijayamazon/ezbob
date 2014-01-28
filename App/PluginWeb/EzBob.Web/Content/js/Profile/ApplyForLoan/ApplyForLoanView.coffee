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

    @currentLoanTypeID = 1 # magic number for backward compatibility.
    @currentRepaymentPeriod = @customer.get('LastApprovedRepaymentPeriod')

    @recalculateThrottled = _.debounce @recalculateSchedule, 250
    @timerId = setInterval _.bind(@refreshTimer, this), 1000
    @model.set "CreditSum": @customer.get("CreditSum"), "OfferValid": @customer.offerValidFormatted()

    
    if(@fixed)
      #@neededCashChanged()
      return

    @model.on "change:neededCash", @neededCashChanged, this
    @isLoanSourceEU = options.model.get "isLoanSourceEU"

  events:
    "click .submit": "submit"
    "change .preAgreementTermsRead": "preAgreementTermsReadChange"
    "change .agreementTermsRead": "showSubmit"
    "change .euAgreementTermsRead": "showSubmit"
    "change .directorConsentRead": "showSubmit"
    "click .download": "download"
    "click .print": "print"

  ui:
    submit: ".submit"
    agreement: ".agreement"
    form: "form"

  preAgreementTermsReadChange: ->
    readPreAgreement = $(".preAgreementTermsRead").is(":checked")
    $(".agreementTermsRead").attr "disabled", not readPreAgreement
    if readPreAgreement
      @$el.find("a[href=\"#tab4\"]").tab "show"
    else
      @$el.find("a[href=\"#tab3\"]").tab "show"
      $(".agreementTermsRead").attr "checked", false
    
    @showSubmit()

  loanSelectionChanged: (e) =>
    @currentRepaymentPeriod = @$('#loan-sliders .period-slider').slider 'value'
    amount = @$('#loan-sliders .amount-slider').slider 'value'

    @model.set "neededCash", parseInt(amount, 10)
    @model.set "loanType", @currentLoanTypeID
    @model.set "repaymentPeriod", @currentRepaymentPeriod

    @neededCashChanged true

  showSubmit: ->
    enabled = EzBob.Validation.checkForm(@validator)
    @model.set "agree", enabled
    @ui.submit.toggleClass "disabled", not enabled

  recalculateSchedule: (args) ->
    val = args.value
    ###
    unless args.reloadSelectedOnly is true
    $.getJSON("#{window.gRootPath}Customer/Schedule/CalculateAll?amount=#{parseInt(val)}").done (data) =>
    for loanKey, offer of data
    $('#loan-type-' + loanKey + ' .Interest').text EzBob.formatPounds offer.TotalInterest
    $('#loan-type-' + loanKey + ' .Total').text EzBob.formatPounds offer.Total
    ###

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
      isShowGift: false
      isShowExportBlock:false
      isShowExceedMaxInterestForSource: false
    )
    scheduleView.render()
    @createAgreementView schedule.Agreement

  neededCashChanged: (reloadSelectedOnly) ->
    @$el.find('.preAgreementTermsRead, .agreementTermsRead, .euAgreementTermsRead').prop 'checked', false
    value = @model.get("neededCash")
    @ui.submit.attr "href", @model.get("url")
    @recalculateThrottled value: value, reloadSelectedOnly: reloadSelectedOnly

  onRender: ->
    if @fixed
        @$(".cash-question").hide()

    if not (@isLoanTypeSelectionAllowed in [ 1, '1' ]) or @isLoanSourceEU
        @$('.duration-select-allowed').hide()

    if not @isLoanSourceEU
        @$('.eu-agreement-section').hide()

    if @model.get 'isCurrentCashRequestFromQuickOffer'
        @$('.loan-amount-header-start').text 'Confirm loan amount'
    else
        @$('.quick-offer-section').hide()

        InitAmountPeriodSliders {
            container: @$('#loan-sliders'),
            amount: { min: @model.get('minCash'), max: @model.get('maxCash'), start: @model.get('maxCash'), step: 100 },
            period: { min: 3, max: 12, start: @model.get('repaymentPeriod'), step: 1, hide: not (@isLoanTypeSelectionAllowed in [ 1, '1' ]) or @isLoanSourceEU },
            callback: (ignored, sEvent) => @loanSelectionChanged() if sEvent is 'change'
        }

    @neededCashChanged()

    @$el.find("img[rel]").setPopover 'right'
    @$el.find("li[rel]").setPopover 'left'
    
    @validator = EzBob.validateLoanLegalForm(@ui.form)
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
    
    enabled = EzBob.Validation.checkForm(@validator)
    if not enabled
        @showSubmit()
        return false 
    
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