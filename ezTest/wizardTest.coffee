exportType = "png"

#baseUrl = "https://localhost:44300"
baseUrl = "https://192.168.0.99/EzBob/EzBob.Web/"
#baseUrl = "https://app.ezbob.com/Customer/Wizard#SignUp"


casper = require('casper').create(
    verbose: true
    logLevel: "debug"
)

h = require('./helpers').create(casper)

casper.start baseUrl, ->
    console.log 'opened.waiting for form'
    casper.waitUntilVisible 'form.signup', ->
        @fill "form.signup",
            EMail: "#{new Date().valueOf()}@gmail.comm"
            signupPass1: "123456"
            signupPass2: "123456"
            SecurityAnswer: "42"
        h.capture "wizard-filled.png"

casper.then ->
    console.log 'clicking button !!!'
    @click '#btn'

casper.then ->
    casper.waitUntilVisible 'div.ebay-logo', null, ->
            console.log 'timeout'
            h.capture('signup-failed.png')
            casper.exit()
        , 10000

casper.then ->
    h.capture "wizard-shops.png"

casper.then ->
    @click '.amazon-logo'

casper.then ->
    h.capture('amazon-instructions.png')
    @fill ".AmazonForm",
        "amazonMerchantId": "A1OXZLJTRHTZJ3"
        "amazonMarketplaceId": "A1F83G8C2ARO7P"
    h.capture('amazon-filled.png')

casper.then ->
    casper.waitWhileSelector '.connect-amazon.disabled', ->
        @click '.connect-amazon'

casper.then ->
    console.log ('waiting for notifications')
    @waitUntilVisible '.notification_green', null, ->
            console.log 'time out'
            h.capture 'amaozon-timeout.png'
            @exit()
        , 10000

casper.then ->
    h.capture 'amazon-added.png'

casper.then ->
    @click '.next'

casper.then ->
    h.capture 'payment-accounts.png'

casper.then ->
    @click '.bank-account-logo'

casper.then ->
    @fill "#bankAccount",
        "AccountNumber" : "70119768"
        "SortCode1" : "20"
        "SortCode2" : "37"
        "SortCode3" : "16"

casper.then ->
    h.capture 'bank-account-filled.png'

casper.then ->
    @click '.connect-bank'

casper.then ->
    console.log ('waiting for notifications')
    @waitUntilVisible '.notification_green', null, ->
            console.log 'time out'
            h.capture 'bank-account-timeout.png'
            @exit()
        , 10000

casper.then ->
    h.capture 'bank-account-added'

casper.then ->
    @click '.next'

casper.then ->
    h.capture 'personal-details.png'

casper.then ->
    #@sendKeys '.addAddressContainer', 'AB101AB'
    @fill 'form[action="/Customer/CustomerDetails/Save"]',
        'FirstName' : 'John'
        'MiddleInitial': 'Roy'
        'Surname': 'Doe'
        'day': '1'
        'month': '10'
        'year': '1970'
        'TimeAtAddress': '3'
        'ConsentToSearch': true
        'DayTimePhone': '0123456789'
        'MobilePhone': '0347593871'
        'Gender': 'M'
        'TypeOfBusiness': 'Entrepreneur'
        'MartialStatus': 'Married'
        'ResidentialStatus': 'Home owner'

casper.then ->
    @evaluate ->
        $('.addAddressContainer input[type="text"]').val('AB101AB').change().keyup()
        $('#OverallTurnOver').val('83764').change().keyup()
        $('#WebSiteTurnOver').val('83764').change().keyup()

casper.then ->
    @click '#PersonalAddress .btn'

casper.then ->
    @waitUntilVisible 'ul.postCodeTextArea li'

casper.then ->
    h.capture 'fill-address.png'
    @click 'ul.postCodeTextArea > li'

casper.then ->
    @click '.postCodeBtnOk'

casper.then ->
    h.capture 'personal-details-filled.png'

casper.then ->
    @click '.btn-continue'

casper.then ->
    @waitUntilVisible '.thankyou-loan'

casper.then ->
    h.capture 'thankyou.png'

#h.clickAndCapture 'a[href="#YourStores"]', 'profile-stores.png'


casper.run ->
    console.log 'casper - done'
    casper.exit()