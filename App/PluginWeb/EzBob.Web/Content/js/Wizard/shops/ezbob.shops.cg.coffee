root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CGAccountButtonView extends EzBob.StoreButtonView
    initialize: (options) ->
        super
            name: options.accountType
            logoText: ''
            shops: @model

    update: ->
        @model.fetch()

class EzBob.CGAccountInfoView extends Backbone.Marionette.ItemView
    events:
        'click a.back': 'back'
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'

    initialize: (options) ->
        @accountType = options.accountType
        @template = '#' + @accountType + 'AccountInfoTemplate'

    inputChanged: =>
        enabled =  EzBob.Validation.checkForm(@validator) 
        @$el.find('a.connect-account').toggleClass('disabled', !enabled)

    getVendorInfo: ->
        if !@vendorInfo
            lst = $.parseJSON $('div#cg-account-list').text()
            @vendorInfo = lst[@accountType]

        @vendorInfo

    connect: =>
        if not EzBob.Validation.checkForm(@validator)
            @validator.form()
            return false
        return false if @$el.find('a.connect-account').hasClass('disabled')

        accountModel = $.parseJSON $('div#cg-account-model-template').text()

        vendorInfo = @getVendorInfo()

        accountModel.accountTypeName = @accountType

        for fi in vendorInfo.SecurityData.Fields
            accountModel[fi.NodeName] = fi.Default if fi.Default

        for propName, propVal of accountModel
            elm = @$el.find '#' + @accountType.toLowerCase() + '_' + propName.toLowerCase()

            if elm.length > 0
                accountModel[propName] = elm.val()

        if vendorInfo.ClientSide.LinkForm.OnBeforeLink.length
             func = new Function 'accountModel', vendorInfo.ClientSide.LinkForm.OnBeforeLink.join "\n"
             accountModel = func.call null, accountModel

             if not accountModel
                 EzBob.App.trigger 'error', vendorInfo.DisplayName + ' Account Data Validation Error'
                 return false

        delete accountModel.id

        acc = new EzBob.CGAccountModel accountModel

        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', vendorInfo.DisplayName + ' Account Saving Error'
            return false

        BlockUi 'on'
        xhr.always =>
            BlockUi 'off'

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            EzBob.App.trigger 'error', 'Failed to Save ' + vendorInfo.DisplayName + ' Account'

        xhr.done (res) =>
            if res.error
                EzBob.App.trigger 'error', res.error
                return false

            try
                @model.add(acc)

            EzBob.App.trigger 'info', vendorInfo.DisplayName + ' Account Added Successfully'

            for propName, propVal of accountModel
                elm = @$el.find '#' + @accountType.toLowerCase() + '_' + propName.toLowerCase()

                if elm.length > 0
                    elm.val("") 

            @inputChanged()
            @trigger('completed');
            @trigger 'back'

        false

    render: =>
        super()

        self = @
        @$el.find('a.connect-account').click((evt) -> self.connect())

        oFieldStatusIcons = $ 'IMG.field_status'
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })

        @validator = EzBob.validateCGShopForm @$el.find('form'), @accountType
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        EzBob.App.trigger 'clear'
        'Link ' + @getVendorInfo().DisplayName + ' Account'

class EzBob.CGAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/CGMarketPlaces/Accounts"

class EzBob.CGAccounts extends Backbone.Collection
    model: EzBob.CGAccountModel

    accountType: ''

    url: -> "#{window.gRootPath}Customer/CGMarketPlaces/Accounts?atn=" + @accountType

    initialize: (data, options) ->
        @accountType = options.accountType
