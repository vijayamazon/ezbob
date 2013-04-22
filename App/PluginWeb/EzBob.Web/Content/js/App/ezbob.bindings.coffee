root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.BidingConverters = EzBob.BidingConverters or {}

EzBob.BidingConverters.floatNumbers = (direction, value) ->
    if value is "" or value is null
        value = 0
    parseFloat(value)

EzBob.BidingConverters.percents = (direction, value) ->
    if value is "" or value is null
        value = 0
    value = parseFloat(value)
    if direction == 'ModelToView'
        return Math.round(value * 100 *100)/100
    else
        return value / 100

EzBob.BidingConverters.notNull = (direction, value) ->
    if value is "" or value is null
        value = 0
    return value

EzBob.BidingConverters.dateTime = (direction, value) ->
    if direction == 'ModelToView'
        moment.utc(value).format('DD/MM/YYYY')
    else
        moment.utc(value, "DD/MM/YYYY").toDate()

EzBob.BidingConverters.percentsFormat = (direction, value) ->
    result = EzBob.BidingConverters.percents(direction, value)
    if direction == 'ModelToView'
        return $.fn.autoNumeric.Format('', result, EzBob.percentFormat)
    else
        return result