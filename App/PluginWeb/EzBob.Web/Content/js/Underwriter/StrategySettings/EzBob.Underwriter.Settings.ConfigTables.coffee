root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings or {}

class EzBob.Underwriter.Settings.LoanOfferMultiplierModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=LoanOfferMultiplier"

class EzBob.Underwriter.Settings.LoanOfferMultiplierView extends Backbone.Marionette.ItemView
    template: "#loan-offer-multiplier-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    events:
        "click .addRange": "addRange"
        "click .removeRange": "removeRange"
        "click #SaveLoanOfferMultiplierSettings": "saveLoanOfferMultiplierSettings"
        "click #CancelLoanOfferMultiplierSettings": "update"
        "change .range-field": "valueChanged"

    valueChanged: (eventObject) ->
        typeIdentifier = eventObject.target.id.substring(0,3)
        if (typeIdentifier == "end")
            id = eventObject.target.id.substring(4)
            newValue = parseInt(eventObject.target.value)
        else
            id = eventObject.target.id.substring(6)
            if (typeIdentifier == "sta")
                newValue = parseInt(eventObject.target.value)
            else
                newValue = parseFloat(eventObject.target.value)

        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.End = newValue
                if (typeIdentifier == "sta")
                    row.Start = newValue
                if (typeIdentifier == "val")
                    row.Value = newValue
                return false
        return false

    saveLoanOfferMultiplierSettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveConfigTable", serializedModels: JSON.stringify(@model.get('configTableEntries')), configTableType: 'LoanOfferMultiplier'
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('loan-offer-multiplier-id')
        index = 0
        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == rangeId)
                ranges.splice(index, 1)
                @render()
                return false

            index++
        return

    addRange: (e, range)->
        freeId = -1
        verified = false
        while (!verified)
            t = @$el.find('#loanOfferMultiplierRow_' + freeId)
            if (t.length == 0)
                verified = true
            else
                freeId--

        this.model.get('configTableEntries').push( {Start: 0, Id: freeId, End: 0, Value:0.0})
        @render()
        return

    serializeData: ->
        data = configTableEntries: @model.get('configTableEntries')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})

        ranges = @model.get('configTableEntries')
        for row in ranges
            startObject = @$el.find('#start_' + row.Id)
            if (startObject.length == 1)
                startObject.numericOnly()
            endObject = @$el.find('#end_' + row.Id)
            if (endObject.length == 1)
                endObject.numericOnly()
            valueObject = @$el.find('#value_' + row.Id)
            if (valueObject.length == 1)
                valueObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()



class EzBob.Underwriter.Settings.BasicInterestRateModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=BasicInterestRate"

class EzBob.Underwriter.Settings.BasicInterestRateView extends Backbone.Marionette.ItemView
    template: "#basic-interest-rate-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    events:
        "click .addRange": "addRange"
        "click .removeRange": "removeRange"
        "click #SaveBasicInterestRateSettings": "saveBasicInterestRateSettings"
        "click #CancelBasicInterestRateSettings": "update"
        "change .range-field": "valueChanged"

    valueChanged: (eventObject) ->
        typeIdentifier = eventObject.target.id.substring(0,3)
        if (typeIdentifier == "end")
            id = eventObject.target.id.substring(4)
            newValue = parseInt(eventObject.target.value)
        else
            id = eventObject.target.id.substring(6)
            if (typeIdentifier == "sta")
                newValue = parseInt(eventObject.target.value)
            else
                newValue = parseFloat(eventObject.target.value)

        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.End = newValue
                if (typeIdentifier == "sta")
                    row.Start = newValue
                if (typeIdentifier == "val")
                    row.Value = newValue
                return false
        return false

    saveBasicInterestRateSettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveConfigTable", serializedModels: JSON.stringify(@model.get('configTableEntries')), configTableType: 'BasicInterestRate'
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('basic-interest-rate-id')
        index = 0
        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == rangeId)
                ranges.splice(index, 1)
                @render()
                return false

            index++
        return

    addRange: (e, range)->
        freeId = -1
        verified = false
        while (!verified)
            t = @$el.find('#basicInterestRateRow_' + freeId)
            if (t.length == 0)
                verified = true
            else
                freeId--

        this.model.get('configTableEntries').push( {Start: 0, Id: freeId, End: 0, Value:0.0})
        @render()
        return

    serializeData: ->
        data = configTableEntries: @model.get('configTableEntries')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})

        ranges = @model.get('configTableEntries')
        for row in ranges
            startObject = @$el.find('#start_' + row.Id)
            if (startObject.length == 1)
                startObject.numericOnly()
            endObject = @$el.find('#end_' + row.Id)
            if (endObject.length == 1)
                endObject.numericOnly()
            valueObject = @$el.find('#value_' + row.Id)
            if (valueObject.length == 1)
                valueObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()



class EzBob.Underwriter.Settings.EuLoanMonthlyInterestModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=EuLoanMonthlyInterest"

class EzBob.Underwriter.Settings.EuLoanMonthlyInterestView extends Backbone.Marionette.ItemView
    template: "#eu-loan-monthly-interest-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    events:
        "click .addRange": "addRange"
        "click .removeRange": "removeRange"
        "click #SaveEuLoanMonthlyInterestSettings": "saveEuLoanMonthlyInterestSettings"
        "click #CancelEuLoanMonthlyInterestSettings": "update"
        "change .range-field": "valueChanged"

    valueChanged: (eventObject) ->
        typeIdentifier = eventObject.target.id.substring(0,3)
        if (typeIdentifier == "end")
            id = eventObject.target.id.substring(4)
            newValue = parseInt(eventObject.target.value)
        else
            id = eventObject.target.id.substring(6)
            if (typeIdentifier == "sta")
                newValue = parseInt(eventObject.target.value)
            else
                newValue = parseFloat(eventObject.target.value)

        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.End = newValue
                if (typeIdentifier == "sta")
                    row.Start = newValue
                if (typeIdentifier == "val")
                    row.Value = newValue
                return false
        return false

    saveEuLoanMonthlyInterestSettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveConfigTable", serializedModels: JSON.stringify(@model.get('configTableEntries')), configTableType: 'EuLoanMonthlyInterest'
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('eu-loan-monthly-interest-id')
        index = 0
        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == rangeId)
                ranges.splice(index, 1)
                @render()
                return false

            index++
        return

    addRange: (e, range)->
        freeId = -1
        verified = false
        while (!verified)
            t = @$el.find('#euLoanMonthlyInterestRow_' + freeId)
            if (t.length == 0)
                verified = true
            else
                freeId--

        this.model.get('configTableEntries').push( {Start: 0, Id: freeId, End: 0, Value:0.0})
        @render()
        return

    serializeData: ->
        data = configTableEntries: @model.get('configTableEntries')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})

        ranges = @model.get('configTableEntries')
        for row in ranges
            startObject = @$el.find('#start_' + row.Id)
            if (startObject.length == 1)
                startObject.numericOnly()
            endObject = @$el.find('#end_' + row.Id)
            if (endObject.length == 1)
                endObject.numericOnly()
            valueObject = @$el.find('#value_' + row.Id)
            if (valueObject.length == 1)
                valueObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()



class EzBob.Underwriter.Settings.DefaultRateCompanyModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=DefaultRateCompany"

class EzBob.Underwriter.Settings.DefaultRateCompanyView extends Backbone.Marionette.ItemView
    template: "#default-rate-company-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    events:
        "click .addRange": "addRange"
        "click .removeRange": "removeRange"
        "click #SaveDefaultRateCompanySettings": "saveDefaultRateCompanySettings"
        "click #CancelDefaultRateCompanySettings": "update"
        "change .range-field": "valueChanged"

    valueChanged: (eventObject) ->
        typeIdentifier = eventObject.target.id.substring(0,3)
        if (typeIdentifier == "end")
            id = eventObject.target.id.substring(4)
            newValue = parseInt(eventObject.target.value)
        else
            id = eventObject.target.id.substring(6)
            if (typeIdentifier == "sta")
                newValue = parseInt(eventObject.target.value)
            else
                newValue = parseFloat(eventObject.target.value)

        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.End = newValue
                if (typeIdentifier == "sta")
                    row.Start = newValue
                if (typeIdentifier == "val")
                    row.Value = newValue
                return false
        return false

    saveDefaultRateCompanySettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveConfigTable", serializedModels: JSON.stringify(@model.get('configTableEntries')), configTableType: 'DefaultRateCompany'
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('default-rate-company-id')
        index = 0
        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == rangeId)
                ranges.splice(index, 1)
                @render()
                return false

            index++
        return

    addRange: (e, range)->
        freeId = -1
        verified = false
        while (!verified)
            t = @$el.find('#defaultRateCompanyRow_' + freeId)
            if (t.length == 0)
                verified = true
            else
                freeId--

        this.model.get('configTableEntries').push( {Start: 0, Id: freeId, End: 0, Value:0.0})
        @render()
        return

    serializeData: ->
        data = configTableEntries: @model.get('configTableEntries')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})

        ranges = @model.get('configTableEntries')
        for row in ranges
            startObject = @$el.find('#start_' + row.Id)
            if (startObject.length == 1)
                startObject.numericOnly()
            endObject = @$el.find('#end_' + row.Id)
            if (endObject.length == 1)
                endObject.numericOnly()
            valueObject = @$el.find('#value_' + row.Id)
            if (valueObject.length == 1)
                valueObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()



class EzBob.Underwriter.Settings.DefaultRateCustomerModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsConfigTable?tableName=DefaultRateCustomer"

class EzBob.Underwriter.Settings.DefaultRateCustomerView extends Backbone.Marionette.ItemView
    template: "#default-rate-customer-settings-template"

    initialize: (options) ->
        @modelBinder = new Backbone.ModelBinder()
        @model.on "reset", @render, @
        @update()
        @

    events:
        "click .addRange": "addRange"
        "click .removeRange": "removeRange"
        "click #SaveDefaultRateCustomerSettings": "saveDefaultRateCustomerSettings"
        "click #CancelDefaultRateCustomerSettings": "update"
        "change .range-field": "valueChanged"

    valueChanged: (eventObject) ->
        typeIdentifier = eventObject.target.id.substring(0,3)
        if (typeIdentifier == "end")
            id = eventObject.target.id.substring(4)
            newValue = parseInt(eventObject.target.value)
        else
            id = eventObject.target.id.substring(6)
            if (typeIdentifier == "sta")
                newValue = parseInt(eventObject.target.value)
            else
                newValue = parseFloat(eventObject.target.value)

        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == id)
                if (typeIdentifier == "end")
                    row.End = newValue
                if (typeIdentifier == "sta")
                    row.Start = newValue
                if (typeIdentifier == "val")
                    row.Value = newValue
                return false
        return false

    saveDefaultRateCustomerSettings: ->
        BlockUi "on"
        xhr = $.post "#{window.gRootPath}Underwriter/StrategySettings/SaveConfigTable", serializedModels: JSON.stringify(@model.get('configTableEntries')), configTableType: 'DefaultRateCustomer'
        xhr.done (res) =>
            if res.error
                 EzBob.App.trigger('error', res.error)
        xhr.always ->
            BlockUi "off"
        false

    removeRange: (eventObject) ->
        rangeId = eventObject.target.getAttribute('default-rate-customer-id')
        index = 0
        ranges = @model.get('configTableEntries')
        for row in ranges
            if (row.Id.toString() == rangeId)
                ranges.splice(index, 1)
                @render()
                return false

            index++
        return

    addRange: (e, range)->
        freeId = -1
        verified = false
        while (!verified)
            t = @$el.find('#defaultRateCustomerRow_' + freeId)
            if (t.length == 0)
                verified = true
            else
                freeId--

        this.model.get('configTableEntries').push( {Start: 0, Id: freeId, End: 0, Value:0.0})
        @render()
        return

    serializeData: ->
        data = configTableEntries: @model.get('configTableEntries')
        return data

    update: ->
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()
            @$el.find("input").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})

        ranges = @model.get('configTableEntries')
        for row in ranges
            startObject = @$el.find('#start_' + row.Id)
            if (startObject.length == 1)
                startObject.numericOnly()
            endObject = @$el.find('#end_' + row.Id)
            if (endObject.length == 1)
                endObject.numericOnly()
            valueObject = @$el.find('#value_' + row.Id)
            if (valueObject.length == 1)
                valueObject.autoNumeric(EzBob.percentFormat).blur()

        return false

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()