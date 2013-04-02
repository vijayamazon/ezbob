class Underwriter
    constructor: (@casper, @url) ->
        @h = require('./helpers').create(@casper, @url)
        this

    open: ->
        @casper.then =>
            @h.logOff()
        @casper.then =>
            @casper.open("#{@url}/Underwriter/Customers")

        @casper.then =>
            @casper.fill 'form[action$="Account/LogOn"]',
                "UserName": "underwriter"
                "Password": "123456"

        @casper.then =>
            @casper.click "input[type='submit']"
        @casper.then =>
            @h.capture "underwriter.png"
        @casper.then =>
            @casper.waitUntilVisible ".left-underwriter-menu"

    openWaiting: ->
        @openTab("waiting")

    openApproved: ->
        @openTab("approved")

    openLate: ->
        @openTab("late")

    openRejected: ->
        @openTab("rejected")

    openAll: ->
        @openTab("all")

    openRegistered: ->
        @openTab("RegisteredCustomers")

    openEscalated: ->
        @openTab("Escalated")

    openTab: (tab) ->
        @casper.then =>
            @casper.click "a[href='#customers/#{tab}']"

        @casper.then =>
            @h.capture "#{tab}.png"

    search: (val) ->
        @casper.then =>
            @casper.sendKeys '.tab-pane.active .ui-jqgrid th input[name="Name"]', val
            @refreshGrid()

    setOfferAmount: (val) ->
        @casper.then =>
            @casper.click "button[name='openCreditLineChangeButton']"
        @casper.then =>
            @casper.waitUntilVisible ".ui-dialog form[name='simpleEditDlg']"
        @casper.then =>
            @casper.fill ".ui-dialog form[name='simpleEditDlg']", {"simpleValueEdit": val}
        @casper.then =>
            @h.capture "changeOfferAmount.png"
        @casper.thenClick ".ui-dialog button.btn"
        @h.waitUnblock()

    refreshGrid: ->
        @casper.evaluate -> EzBob.App.vent.trigger('uw:grids:performsearch')

    searchAndWait: (val) ->
        @casper.then => @search(val)
        @casper.then =>
            query = ".tab-pane.active .ui-jqgrid td:contains(#{val})"
            waitFunc = () =>
                @casper.log "wwwait " + query
                @h.capture "wait.png"
                evalFunc = (query) ->
                    return $(query).length > 0
                @casper.evaluate  evalFunc, query

            @casper.waitFor waitFunc, null, null, 30000

    openCustomer: ->
        @casper.then =>
            @casper.click ".profileLink"
        @casper.then =>
            @casper.waitUntilVisible "#profile-summary"
        @casper.then =>
            @h.waitUnblock()
            @casper.waitUntilVisible ".profile-person-info > table"
            @casper.waitUntilVisible ".profile-loan-info > table"
        @casper.then =>
            @h.capture "customer.png"

    approve: (comment) ->
        @casper.then =>
            @casper.log "approving customer"
        @casper.thenClick "#ApproveBtn"
        @casper.then =>
            @h.capture "approving.png"
        @casper.then =>
            func = (val) ->
                $('#functionsPopup textarea').val(val)
            @casper.evaluate func, comment
        @casper.thenClick "input[value='Approve']"
        @h.waitUnblock()
        @casper.then =>
            @h.capture "approved.png"



exports.create = (casper, url) ->
    new Underwriter(casper, url)
