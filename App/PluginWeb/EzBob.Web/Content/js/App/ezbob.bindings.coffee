root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.BindingConverters = EzBob.BindingConverters or {}

EzBob.BindingConverters.floatNumbers = (direction, value) ->
    if value is "" or value is null
        value = 0
    parseFloat(value)

EzBob.BindingConverters.percents = (direction, value) ->
    if value is "" or value is null
        value = 0
    value = parseFloat(value)
    if direction == 'ModelToView'
        return Math.round(value * 100 *100)/100
    else
        return value / 100

EzBob.BindingConverters.notNull = (direction, value) ->
    if value is "" or value is null
        value = 0
    return value

EzBob.BindingConverters.dateTime = (direction, value) ->
    if direction == 'ModelToView'
        moment.utc(value).format('DD/MM/YYYY')
    else
        moment.utc(value, "DD/MM/YYYY").toDate()

EzBob.BindingConverters.autonumerciFormat = (format) ->
    return (direction, value) ->
        if direction == 'ModelToView'
            return EzBob.formatPoundsFormat(value, format)
        else
            return $.autoNumeric.Strip($("<input/>").val(value), format)

EzBob.BindingConverters.percentsFormat = (direction, value) ->
        if direction == 'ModelToView'
            value = EzBob.BindingConverters.percents(direction, value)
            result = EzBob.BindingConverters.autonumerciFormat(EzBob.percentFormat)(direction, value)
            return result
        else
            value = EzBob.BindingConverters.autonumerciFormat(EzBob.percentFormat)(direction, value)
            result = EzBob.BindingConverters.percents(direction, value)
            return result

EzBob.BindingConverters.moneyFormat = EzBob.BindingConverters.autonumerciFormat(EzBob.moneyFormat)

Backbone.Collection::safeFetch = ->
    
    loggedIn = -> return $('body').hasClass('auth')
    
    if loggedIn()
        return @fetch()
    def = $.Deferred()
    setTimeout (-> def.resolve().promise()), 1
    return def
