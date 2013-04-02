Faker = require('./Faker')

casper = require('casper').create(
    verbose: true
    logLevel: "debug"
    waitTimeout: 15000,
    viewportSize: {width: 1366, height: 768 },
)

baseUrl = "https://192.168.0.99/EzBob/EzBob.Web"
#baseUrl = "https://localhost:44300"

h = require('./helpers').create(casper, baseUrl)
w = require('./wizard').create(casper, baseUrl)
u = require('./underwriter').create(casper, baseUrl)

customer = {
        email       : "auto.#{Faker.Internet.email()}"
        password    : "121212"
        firstName   : Faker.Name.firstName()
        surname     : Faker.Name.lastName()
        phone       : Faker.Helpers.replaceSymbolWithNumber("0##########")
        mobile      : Faker.Helpers.replaceSymbolWithNumber("0##########")
        answer      : "1212"
        ekmShops: [
                {login: "EKMShop1", password: "121212"},
                {login: "EKMShop2", password: "121212"}
            ]
        amazonShops: [
                {amazonMerchantId: "A1OXZLJTRHTZJ3",  amazonMarketplaceId: "A1F83G8C2ARO7P"}
            ]
        ebayShops: [
                {userid: "tisichenko",  pass: "zJ=x=38-f"}
            ]
        paypalAccounts:[
            { login_email: "andreykaydash@gmail.com", login_password: "hjvfirf777fylhtq" }
        ]
        bankAccount : {number: "70119768", code: ["20", "37", "16"], type: "personal"}

    }

casper.start baseUrl
w.registerCustomer(customer)

casper.run -> casper.exit()