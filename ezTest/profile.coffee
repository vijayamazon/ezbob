class Profile
    constructor: (@casper, @url) ->
        @h = require('./helpers').create(@casper, @url)
        @

    open: (customer) ->
        @casper.thenOpen ("#{@url}/Customer/Profile")
        @casper.then =>
            @casper.waitUntilVisible "form[action$='Account/LogOn']"
        @casper.then =>
            @casper.fill "form[action$='Account/LogOn']", 
                "UserName" : customer.email
                "Password" : customer.password
        @casper.then =>
            @casper.click "input[type='submit']"
        @casper.then =>
            @h.capture ("profile.png")
            @h.log "opening customer's profile"
            @casper.waitUntilVisible "#profile-main"
            @h.capture ("profile.png")

    takeLoan: (amount) ->
        @casper.then =>
            @h.log "taking a loan"
            @casper.click ".get-cash"
        @casper.then =>
            @h.waitUnblock()
        @casper.then =>
            @casper.waitUntilVisible ".preAgreementTermsRead"
        @h.thenCapture ("loan.png")
        @casper.thenEvaluate (money) -> $('.get-cash-amount').val(85).focus().blur()
        @casper.then =>
            @h.waitUnblock()
        @h.thenCapture ("loan.png")
        @casper.thenClick ".preAgreementTermsRead"
        @casper.thenClick ".agreementTermsRead"
        @h.thenCapture ("loan.png")
        @casper.thenClick ".submit.btn-continue[href*='GetCash']"
        @h.thenCapture ("loan-taken.png")
        @casper.then =>
            @casper.waitUntilVisible "form.paypont-form"
        @h.thenCapture ("paypoint.png")
        @casper.then =>
            @casper.fill "form.paypont-form",
                "customer"      : "alex"
                "card_type"     : "Visa"
                "card_no"       : "4444333322221111"
                "cv2"           : "123"
                "expiry"        : "11/15"
        @casper.then =>
            @casper.evaluate => $("input[name='expiry']").change().focus().blur()
        @casper.then =>
            @h.capture ("paypoint-filled.png")
        @casper.then =>
            @casper.click "input[name='accept']"
        @casper.then =>
            @casper.waitUntilVisible ".payment-schedule-table"
        @casper.then =>
            @lastLoanId = @casper.getElementAttribute('#loanid', 'value')
        @casper.then =>
            @h.capture ("loan-taken.png")
        @casper.then =>
            @casper.click "a[href='/Customer/Profile']"
        @casper.then =>
            @casper.waitUntilVisible "#profile-main"
            @h.capture ("profile.png")

    openLoan: ->
        @casper.then =>
            @casper.click "a[href='#LoanDetails/#{@lastLoanId}']"
        @casper.then =>
            @casper.waitUntilVisible  "table.payment-schedule-table"
        @casper.then =>
            @casper.waitUntilVisible  "a[href*='/Agreements/Download/']"
        @casper.then =>
            @h.capture "loan-details.png"
        return

    payLoan: () ->
        @casper.then =>
            @casper.click "a[href*='#PayEarly']"
        @casper.then =>
            @casper.waitUntilVisible ".early-payment-form"
            @h.capture "payearly.png"
        @casper.then =>
            @casper.click ".profile-screen .submit"
        @casper.then =>
            @casper.waitUntilVisible ".by-paying-early"
            @h.capture "paid.png"
        @casper.then =>
            @casper.click "a[href*='/Customer/Profile']"
        @casper.then =>
            @casper.waitUntilVisible  "a[href*='/Agreements/Download/']"
            @h.capture ("loan-info.png")

exports.create = (casper, url) ->
    new Profile(casper, url)

