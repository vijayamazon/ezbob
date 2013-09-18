root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Installment extends Backbone.Model
    defaults:
        IsAdding: false
        skipRecalculations: false

    initialize: ->
        @on "change:Balance", @balanceChanged, this
        @on "change:Principal", @principalChanged, this
        @on "change:Total", @totalChanged, this
        @on "change:Date", @dateChanged, this

    balanceChanged: ->
        @safeRecalculate ->
            return if @get('Balance') is @previous('Balance')
            principal = @round((@get('BalanceBeforeRepayment') - @get('Balance')))
            @set('Principal', principal)
            @recalculate()

    totalChanged: ->
        @safeRecalculate ->
            diff = @get("Total") - @previous("Total")
            return if diff is 0
            @set('Balance', (@get("Balance") - diff))
            @set('Principal', (@get("Principal") + diff))

    principalChanged: ->
        @safeRecalculate ->
            diff = @get("Principal") - @previous("Principal")
            return if diff is 0
            @set('Balance', (@get("Balance") - diff))
            @recalculate()

    safeRecalculate: (func, params...) ->
        return if @skipRecalculations
        @skipRecalculations = true
        func.call(this, params)
        @skipRecalculations = false

    recalculate: ->
        @set {'Total': @get('Principal') + @get('Interest') + @get('Fees')}

    dateChanged: ->
        #console.log(@get("Date"))
    round: (number) ->
            number = Math.round(number * 100)
            number = number / 100


class EzBob.Installments extends Backbone.Collection
    model: EzBob.Installment
    comparator: (m1, m2) -> 
        d1 = moment.utc(m1.get('Date')).startOf('day')
        d2 = moment.utc(m2.get('Date')).startOf('day')
        d = d1.diff d2, "days"
        
        if d < 0
            r = -1
        else if d == 0
            r = 0
        else r = 1

        if r == 0 && m1.get("Type") != m2.get("Type")
            if m1.get("Type") == "Installment"
                r = 1
            else
                r = -1

        return r

class EzBob.LoanModel extends Backbone.Model
    url: -> "#{window.gRootPath}Underwriter/LoanEditor/Loan/#{@get('Id')}"

    initialize: ->
        items = new EzBob.Installments()
        items.on "change", @itemsChanged, @
        @set "Items", items

    itemsChanged: ->
        @trigger "change"

    shiftDate: (installment, newDate, oldDate) ->
        newDate = moment.utc(newDate)
        oldDate = moment.utc(oldDate)
        diff = newDate - oldDate
        index = @get("Items").indexOf installment
        for item, i in @get("Items").models when i > index
            d1 = moment.utc(item.get("Date"))
            d2 = moment.utc(item.get("Date")).add(diff)
            item.set("Date", moment.utc(item.get("Date")).add(diff).toDate())
        false

    shiftInterestRate: (installment, rate) ->
        index = @get("Items").indexOf installment
        for item, i in @get("Items").models when i > index
            item.set("InterestRate", rate)
        false


    parse : (r, o) ->
        @get("Items").reset(r.Items)
        delete r.Items
        return r

    toJSON: ->
        r = super()
        r.Items = r.Items.toJSON()
        return r

    removeItem: (index) ->
        items = @get "Items"
        items.remove items.at(index)
        @recalculate()

    addInstallment: (installment) ->
        @get("Items").add(installment)
        @recalculate()

    addFee: (fee) ->
        @get("Items").add(fee)
        @recalculate()
        return

    recalculate: ->
        @save({}, {url: "#{window.gRootPath}Underwriter/LoanEditor/Recalculate/#{@get('Id')}"})

    addFreezeInterval: (sStartDate, sEndDate, nRate) ->
        @save({}, {url: "#{window.gRootPath}Underwriter/LoanEditor/AddFreezeInterval/#{@get('Id')}?startdate=#{sStartDate}&enddate=#{sEndDate}&rate=#{nRate}"})

    removeFreezeInterval: (intervalId) ->
        @save({}, {url: "#{window.gRootPath}Underwriter/LoanEditor/RemoveFreezeInterval/#{@get('Id')}?intervalid=#{intervalId}"})

    getInstallmentBefore: (date) ->
        date = moment.utc(date).toDate()
        installment = null
        for item in @get("Items").models when item.get("Type") is "Installment" and moment.utc(item.get("Date")).toDate() < date
            installment = item
        return installment

    getInstallmentAfter: (date) ->
        date = moment.utc(date).toDate()
        for item in @get("Items").models when item.get("Type") is "Installment" and moment.utc(item.get("Date")).toDate() > date
            return item
        return null

class EzBob.LoanModelTemplate extends EzBob.LoanModel
    url: -> "#{window.gRootPath}Underwriter/LoanEditor/LoanCR/#{@get('CashRequestId')}"
    recalculate: ->
        @save({}, {url: "#{window.gRootPath}Underwriter/LoanEditor/RecalculateCR"})