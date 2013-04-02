exportType = "png"

baseUrl = "https://localhost:44300"
#baseUrl = "https://app.ezbob.com/Customer/Wizard#SignUp"


casper = require('casper').create()
h = require('./helpers').create(casper)

casper.start baseUrl, ->
    console.log 'opened.waiting for form'
    casper.waitUntilVisible 'form[action="/Account/SignUp"]', ->
        console.log 'got form. capturing'
        h.capture "start.png"
        console.log 'logging in'
        @click('[href="/Account/LogOn"]')

casper.then ->
    console.log "clicked login, new location is #{@getCurrentUrl()}"
    h.capture "login.png"
    @fill "form[action='/Account/LogOn']",
        UserName: "demo1@gmail.comm"
        Password: "123456"
    h.capture "filled.png"

casper.then ->
    @click 'input[type="submit"]'

casper.then ->
    console.log "clicked login, new location is #{@getCurrentUrl()}"
    h.capture "profile-activity.png"


h.clickAndCapture 'a[href="#YourStores"]', 'profile-stores.png'
h.clickAndCapture 'a[href="#PaymentAccounts"]', 'profile-accounts.png'
h.clickAndCapture 'a[href="#YourDetails"]', 'profile-your-details.png'
h.clickAndCapture 'a[href="#Settings"]', 'profile-settings.png'

h.clickAndCapture 'a.edit-password', 'profile-edit-password.png'
h.clickAndCapture '#change-password a.back', 'profile-edit-password-back.png'

h.clickAndCapture 'a.edit-question', 'profile-edit-question.png'
h.clickAndCapture '#change-password a.back', 'profile-edit-question-back.png'

casper.run ->
    console.log 'casper - done'
    casper.exit()