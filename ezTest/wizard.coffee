class Wizard
    constructor: (@casper, @url) ->
        @h = require('./helpers').create(@casper, @url)
        @

    registerCustomer : (customer) ->
        @h.logOff()
        @h.setTestMode()
        @createCustomer(customer)
        @addEbayShops(customer)
        @addEKMShops(customer)
        @addAmazonShops(customer)
        @addPaypalAccounts(customer)
        @addBankAccount(customer)
        @fillPersonallInformation(customer)
        @complete()

    createCustomer: (customer) ->
        @casper.then =>
            @h.log "Create customer #{customer.email}"
            @casper.waitUntilVisible 'form.signup', =>
                @casper.log "filling customer details"
                @casper.fill "form.signup",
                    EMail: customer.email
                    signupPass1: customer.password
                    signupPass2: customer.password
                    SecurityAnswer: customer.answer
                @h.capture "wizard-filled.png"
            @casper.then =>
                @casper.click '#btn'
            @casper.then =>
                @h.waitUnblock()
            @casper.then =>
                @h.capture "customer-created.png"

    addPaypalAccounts: (customer) ->
        return unless customer.paypalAccounts
        for account in customer.paypalAccounts
            @addPaypalAccount(account)


    addEbayShops: (customer) ->
        return unless customer.ebayShops
        for shop in customer.ebayShops
            @addEbayShop(shop)

    addEKMShops: (customer) ->
        return unless customer.ekmShops
        for shop in customer.ekmShops
            @addEKMShop(shop)

    addAmazonShops: (customer) ->
        return unless customer.amazonShops
        for shop in customer.amazonShops
            @addAmazonShop(shop)

    addPaypalAccount: (account) ->
        @casper.then =>
            @h.log "Add paypal shop #{account.login_email}"
            @casper.waitUntilVisible '.paypal-logo', null, => @casper.click '.next'
        @casper.then =>
            @casper.waitUntilVisible '.paypal-logo', null, => @h.capture "paypal-logo-waiting-failed.png"
        @casper.then =>
            @h.capture "payment-accounts.png"
            @casper.click '.paypal-logo'
        @casper.then =>
            #payment accounts div class has name "ebay instruction"
            @casper.waitUntilVisible ".ebay-instructions", null, => @h.capture "pay-pal-instructions-waiting-failed.png"
        @casper.then =>
            @casper.evaluate ()=>
                [].forEach.call __utils__.findAll('a'), (link)=> 
                    link.removeAttribute('target')
        @casper.then =>
            @casper.click ".connect-paypal"
        @casper.then =>
            @casper.waitUntilVisible 'form',null, => @h.capture "paypal-form-waiting-fail.png"
        @casper.then =>
            @h.capture "paypal-page.png"
            @casper.fill 'form',
                login_email: account.login_email
                login_password: account.login_password
            , true
        @casper.then =>
            @h.capture "paypal-filled.png"
            @casper.waitUntilVisible '#apiGrantPermission', null, => @h.capture "ebay-submit-waiting-failed.png"
        @casper.then =>
            @casper.click '[type="submit"]'
        @casper.then =>
            @casper.waitUntilVisible '#ezbob_logo', null, => @h.capture "ebay-waiting-failed.png"
        @casper.then =>
            @casper.thenOpen @url
        @casper.then =>
            @casper.waitUntilVisible '#ezbob_logo', null, => @h.capture "ebay-waiting-failed.png"
        @casper.then =>
            @h.capture "paypal-added.png"

    addEbayShop: (shop) ->
        @casper.then =>
            @h.log "Add ebay shop #{shop.userid}"
            @casper.evaluate =>
                document.location.replace("#{@url}/Customer/Wizard#ShopInfo")
        @casper.then =>
            @casper.waitUntilVisible '.ebay-logo', null, => @h.capture "ebay-logo-waiting-failed.png"
        @casper.then =>
            @h.capture "shops.png"
            @casper.click '.ebay-logo'
        @casper.then =>
            @casper.waitUntilVisible ".ebay-instructions"    
        @casper.then =>
            @casper.evaluate ()=>
                [].forEach.call __utils__.findAll('a'), (link)=> 
                    link.removeAttribute('target')
        @casper.then =>
            @casper.click ".connect-ebay"
        @casper.then =>
            @casper.waitUntilVisible '#SignInForm',null, => @h.capture "ebay-fail.png"
        @casper.then =>
            @h.capture "ebay-page.png"
            @casper.fill '#SignInForm',
                userid: shop.userid
                pass: shop.pass
            , true
        @casper.then =>
            @h.capture "ebay-filled.png"
            @casper.waitUntilVisible '#frmAuth', null, => @h.capture "ebay-submit-waiting-failed.png"
        @casper.then =>
            @casper.click '[type="submit"]'
        @casper.then =>
            @casper.waitUntilVisible '#ezbob_logo', null, => @h.capture "ebay-waiting-failed.png"
        @casper.thenOpen @url
        @casper.then =>
            @casper.waitUntilVisible '.ezbob_logo', null, => @h.capture "ebay-waiting-failed.png"
        @casper.then =>
            @h.capture "ebay-waiting-success.png"
            @casper.click '.ebay-logo'
        @casper.then =>
            @casper.waitUntilVisible ".ebay-instructions"    
            @h.capture "ebay-instructions.png"
        @casper.then =>
            @casper.evaluate (userid)=>
                window.AlertToken userid
            , shop.userid
        @casper.then =>
            @casper.click ".back"
        @casper.then =>
            @casper.waitUntilVisible '.notification_green'
        @casper.then =>
            @h.capture "ebay-added.png"

    addEKMShop: (shop) ->
        @casper.then =>
            @h.log "Add ekm shop #{shop.login}"
        @casper.then =>
            @casper.waitUntilVisible '.ebay-logo'
        @casper.then =>
            @h.capture "shops.png"
            @casper.click '.ekm-logo'
        @casper.then =>
            @casper.waitUntilVisible "#ekmAccount"
        @casper.then =>
            @h.capture "ekm.png"
            @casper.fill "#ekmAccount",
                ekm_login: shop.login
                ekm_password: shop.password
        @casper.then =>
            @h.capture "ekm-filled.png"
            @casper.click ".connect-ekm"
        @casper.then =>
            @casper.waitUntilVisible '.notification_green'

    addAmazonShop: (shop) ->
        @casper.then =>
            @h.log "Add Amazon shop #{shop.amazonMerchantId}"
            @casper.waitUntilVisible '.amazon-logo'
        @casper.then =>
            @casper.click '.amazon-logo'
            @casper.click '.amazon-logo'
        @casper.then =>
            @casper.waitUntilVisible "#amazonMerchantId"
        @casper.then =>
            @h.capture "amazon.png"
            @casper.fill ".AmazonForm",
                amazonMerchantId: shop.amazonMerchantId
                amazonMarketplaceId: shop.amazonMarketplaceId
        @casper.then =>
            @h.capture "amazon-filled.png"
            @casper.click ".connect-amazon"
        @casper.then =>
            @casper.waitUntilVisible '.notification_green'

    addBankAccount: (customer) ->
        @casper.then =>
            @h.log "Add bank account #{customer.bankAccount.number}"
            @casper.waitUntilVisible '.bank-account-logo', null, => @casper.click '.next'

        @casper.then =>
            @h.capture 'payment-accounts.png'

        @casper.then =>
            @casper.click '.bank-account-logo'

        @casper.then =>
            @casper.fill "#bankAccount",
                "AccountNumber" : customer.bankAccount.number
                "SortCode1"     : customer.bankAccount.code[0]
                "SortCode2"     : customer.bankAccount.code[1]
                "SortCode3"     : customer.bankAccount.code[2]

        @casper.then =>
            @h.capture 'bank-account-filled.png'

        @casper.then =>
            @casper.click '#baPersonal'

        @casper.then =>
            @casper.click '.connect-bank'

        @casper.then =>
            @casper.waitUntilVisible '.notification_green'

    fillPersonallInformation: (customer) ->
        @casper.then =>
            @h.log "Fill personal information for #{customer.firstName} #{customer.surname}"
            @casper.waitUntilVisible '.CompanyDetailForm', null, => @casper.click '.next'
        @casper.then =>
            @h.capture 'personal-details.png'

        @casper.then =>
            @casper.fill 'form[action$="/Customer/CustomerDetails/Save"]',
                'FirstName'         : customer.firstName
                'MiddleInitial'     : 'Roy'
                'Surname'           : customer.surname
                'day'               : '1'
                'month'             : '10'
                'year'              : '1970'
                'TimeAtAddress'     : '3'
                'ConsentToSearch'   : true
                'DayTimePhone'      : customer.phone
                'MobilePhone'       : customer.mobile
                'Gender'            : 'M'
                'TypeOfBusiness'    : 'Entrepreneur'
                'MartialStatus'     : 'Married'
                'ResidentialStatus' : 'Home owner'

        @casper.then =>
            @casper.evaluate ->
                $('#OverallTurnOver').val('83764').change().keyup()
                $('#WebSiteTurnOver').val('83764').change().keyup()
            @fillAddress()

        @casper.then =>
            @h.capture 'personal-details-filled.png'

    fillAddress: ->
            @h.log "Filling address"
            @casper.evaluate ->
                $('.addAddressContainer input[type="text"]').val('AB101AB').change().keyup()
            @casper.then =>
                @casper.click '#PersonalAddress .btn'

            @casper.then =>
                @casper.waitUntilVisible 'ul.postCodeTextArea li'

            @casper.then =>
                @h.capture 'fill-address.png'
                @casper.click 'ul.postCodeTextArea > li'

            @casper.then =>
                @casper.click '.postCodeBtnOk'

    complete: (customer) ->
        @casper.then =>
            @casper.click '.btn-continue'
        @casper.then =>
            @casper.waitUntilVisible '.thankyou-loan'

        @casper.then =>
            @h.capture 'thankyou.png'

        @casper.then =>
            @casper.click ".btn-continue"

        @casper.then =>
            @casper.waitUntilVisible "#profile-main"

        @casper.then =>
            @h.capture "profile.png"

exports.create = (casper, url) ->
    new Wizard(casper, url)

