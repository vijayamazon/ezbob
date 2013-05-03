﻿root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.ApplyForLoanModel extends Backbone.Model
  defaults:
    neededCash: 100
    maxCash: 15000
    minCash: EzBob.Config.MinLoan
    agree: false
    agreement: false
    CreditSum: 0
    OfferValid: 0
    OfferValidMintes: 0

  validate: (attrs) ->
    unless typeof attrs.neededCash is "undefined"
      val = attrs.neededCash
      attrs.neededCash = @get("minCash")  if isNaN(val)
      attrs.neededCash = @get("maxCash")  if val > @get("maxCash")
      attrs.neededCash = @get("minCash")  if val < @get("minCash")
    false

  initialize: ->
    @on "change:neededCash", @buildUrl, this
    @set
      neededCash: @get("maxCash")
      minCash: (if @get("maxCash") > EzBob.Config.MinLoan then EzBob.Config.MinLoan else EzBob.Config.XMinLoan)

  buildUrl: ->
    @set "url", "GetCash/GetTransactionId?loan_amount=#{@get('neededCash')}"