Faker = require('./Faker')

casper = require('casper').create(
    verbose: true
    logLevel: "debug"
    waitTimeout: 20000,
    viewportSize: {width: 1366, height: 768 }
)

baseUrl = "https://localhost:44300"

h = require('./helpers').create(casper, baseUrl)
w = require('./wizard').create(casper, baseUrl)
u = require('./underwriter').create(casper, baseUrl)
p = require('./profile').create(casper, baseUrl)

customer = {
        email       : "auto.#{Faker.Internet.email()}" #"#{new Date().valueOf()}@gmail.comm"
        password    : "121212"
        firstName   : Faker.Name.firstName()
        surname     : Faker.Name.lastName()
        phone       : Faker.Helpers.replaceSymbolWithNumber("0##########")
        mobile      : Faker.Helpers.replaceSymbolWithNumber("0##########")
        answer      : "1212"
        ekmShops: [
                {login: "EKMShop1", password: "121212"},
                {login: "EKMShop2", password: "121212"}
            ],
        bankAccount : {number: "70119768", code: ["20", "37", "16"], type: "personal"}
    }

casper.start baseUrl

w.registerCustomer(customer)

u.open()
u.openWaiting()
u.searchAndWait(customer.email)
u.openCustomer()
u.setOfferAmount(1234)
u.approve(Faker.Lorem.sentence())

h.logOff()
p.open(customer)
loanId = p.takeLoan(876)
p.openLoan()
p.payLoan()

casper.run -> casper.exit()