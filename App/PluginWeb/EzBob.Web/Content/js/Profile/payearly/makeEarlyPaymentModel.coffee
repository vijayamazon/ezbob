root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}


class EzBob.Profile.MakeEarlyPaymentModel extends Backbone.Model
  defaults:
    amount: 0
    paymentType: "loan" #{loan, total, rollover, totalLate}
    loanPaymentType: "full" #{full, next, other, late}
    rolloverPaymentType: "minimum" #{minimum, other}
    defaultCard: true
    url: "#"

  initialize: ->
    @get("customer").on "fetch", @recalculate, this
    @on "change:amount change:paymentType change:loan change:loanPaymentType", @changed, this
    @on "change:paymentType", @paymentTypeChanged, this
    @on "change:loanPaymentType", @loanPaymentTypeChanged, this
    @on "change:rolloverPaymentType", @rolloverPaymentTypeChanged, this
    @on "change:loan", @loanChanged, this
    @recalculate()

  recalculate: ->
    customer = @get("customer")    
    liveRollovers = customer.get("ActiveRollovers")
    liveLoans = customer.get("ActiveLoans")
    total = customer.get("TotalEarlyPayment")
    loan= (if liveLoans then liveLoans[0] else null)
    currentRollover = @calcCurrentRollover loan

    @set
      total: total
      liveLoans: liveLoans
      rollovers: liveRollovers
      loan: loan
      amount: customer.get("TotalEarlyPayment")
      currentRollover: currentRollover
      hasLateLoans: customer.get("hasLateLoans")
      totalLatePayment: customer.get("TotalLatePayment")
      paymentType: if liveRollovers.length>0 then "rollover" else "loan"

  calcCurrentRollover: (loan)->
    rollovers = @get("customer").get("ActiveRollovers").toJSON()
    currentRollover = _.where(rollovers, {LoanId: loan && loan.get('Id') })[0] || null
    return currentRollover

  paymentTypeChanged: (e)->
    type = @get("paymentType")
    switch type
      when "total"
        @set "amount", @get("customer").get("TotalEarlyPayment")
      when "totalLate"
        @set "amount", @get("customer").get("TotalLatePayment")
      when "loan"
        loan = @get("loan")
        @set "loanPaymentType", "full"  if loan and @get("liveLoans").length>1
      when "rollover"
        @set "rolloverPaymentType", "minimum"
      else

  loanChanged: ->
    currentRollover = (@calcCurrentRollover @get "loan") || null
    status =  @get("loan") and @get("loan").get "Status"
    @set 
        currentRollover: currentRollover
        loanPaymentType: if status == "Late" then  "late" else "full"
    @set amount: currentRollover && currentRollover.RolloverPayValue if currentRollover

  rolloverPaymentTypeChanged: ->
    @set "amount", @get("currentRollover") && @get("currentRollover").RolloverPayValue

  loanPaymentTypeChanged: ->
    type = @get("loanPaymentType")
    loan = @get("loan")
    amount = 0
    return  unless loan
    switch type
      when "full"
        amount = loan.get("TotalEarlyPayment")
      when "next"
        amount = loan.get("NextEarlyPayment")
      when "late"
        amount = loan.get("AmountDue")
      when "other"
        amount = loan.get("TotalEarlyPayment")
      else
    @set "amount", amount

  changed: ->
    loan = @get("loan")
    return  unless loan
    url = window.gRootPath + "Customer/Paypoint/Pay?amount=" + @get("amount")
    url += "&type=" + @get("paymentType")
    url += "&paymentType=" + if @get("paymentType") != "rollover" then @get("loanPaymentType") else @get("rolloverPaymentType")
    url += "&loanId=" + loan.id
    url += "&rolloverId=" + ( if @get("currentRollover") is not null then @get("currentRollover").Id else -1)
    @set url: url