root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CGAccountInfoView extends Backbone.Marionette.ItemView
    events:
        'click a.back': 'back'
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'

    initialize: (options) ->
        @uploadFilesDlg = null
        @accountType = options.accountType
        @template = '#' + @accountType + 'AccountInfoTemplate'

    inputChanged: =>
        enabled = EzBob.Validation.checkForm(@validator) 
        @$el.find('a.connect-account').toggleClass('disabled', !enabled)

    getVendorInfo: ->
        if !@vendorInfo
            lst = $.parseJSON $('div#cg-account-list').text()
            @vendorInfo = lst[@accountType]

        @vendorInfo

    buildModel: (bUploadMode) =>
        accountModel = $.parseJSON $('div#cg-account-model-template').text()

        oVendorInfo = @getVendorInfo()

        accountModel.accountTypeName = @accountType

        for fi in oVendorInfo.SecurityData.Fields
            accountModel[fi.NodeName] = fi.Default if fi.Default

        for propName, propVal of accountModel
            elm = @$el.find '#' + @accountType.toLowerCase() + '_' + propName.toLowerCase()

            if elm.length > 0
                accountModel[propName] = elm.val()

        if oVendorInfo.ClientSide.LinkForm.OnBeforeLink.length
             func = new Function 'accountModel', 'bUploadMode', oVendorInfo.ClientSide.LinkForm.OnBeforeLink.join "\n"
             accountModel = func.call null, accountModel, bUploadMode

             if not accountModel
                 return null

        delete accountModel.id

        return accountModel

    connect: =>
        if not EzBob.Validation.checkForm(@validator)
            @validator.form()
            return false

        return false if @$el.find('a.connect-account').hasClass('disabled')

        accountModel = @buildModel false

        oVendorInfo = @getVendorInfo()

        if not accountModel
            EzBob.App.trigger 'error', oVendorInfo.DisplayName + ' Account Data Validation Error'
            return false

        acc = new EzBob.CGAccountModel accountModel

        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', oVendorInfo.DisplayName + ' Account Saving Error'
            return false

        BlockUi 'on'
        xhr.always =>
            BlockUi 'off'

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            EzBob.App.trigger 'error', 'Failed to Save ' + oVendorInfo.DisplayName + ' Account'

        xhr.done (res) =>
            if res.error
                EzBob.App.trigger 'error', res.error
                return false

            try
                @model.add(acc)

            EzBob.App.trigger 'info', oVendorInfo.DisplayName + ' Account Added Successfully'

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

        @validator = EzBob.validateCGShopForm @$el.find('form'), @accountType

        EzBob.UiAction.registerView @
        @
    # end of render

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
