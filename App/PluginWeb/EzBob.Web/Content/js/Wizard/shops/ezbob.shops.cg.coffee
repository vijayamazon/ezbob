root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CGAccountButtonView extends EzBob.StoreButtonView
    initialize: (options) ->
        super
            name: options.accountType
            logoText: ''
            shops: @model

    update: ->
        @model.fetch().done -> EzBob.App.trigger 'ct:storebase.shop.connected'

class EzBob.CGAccountInfoView extends Backbone.Marionette.ItemView
    events:
        'click a.back': 'back'
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'
        'click .upload-files': 'uploadFiles'

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

    uploadFiles: =>
        sKey = 'f' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000)
        sModelKey = 'model' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000)

        while window[sKey]
            sKey += Math.floor(Math.random() * 1000)

        while window[sModelKey]
            sModelKey += Math.floor(Math.random() * 1000)

        oVendorInfo = @getVendorInfo()

        window[sModelKey] = =>
            return @buildModel true

        window[sKey] = (sResult) =>
            delete window[sKey]
            delete window[sModelKey]

            @uploadFileDlg.dialog 'close'
            @uploadFileDlg = null

            oResult = JSON.parse sResult

            if oResult.error
                EzBob.App.trigger 'error', 'Problem Linking ' + oVendorInfo.DisplayName + ' Account: ' + oResult.error.Data.error
            else
                if oResult.submitted
                    EzBob.App.trigger 'info', oVendorInfo.DisplayName + ' Account Added Successfully'

            @trigger 'completed'
            @trigger 'back'

        $('iframe', @$el.find('div#upload-files-form')).each (idx, iframe) ->
            iframe.setAttribute 'width', 570
            iframe.setAttribute 'height', 515
            iframe.setAttribute 'src', "#{window.gRootPath}Customer/CGMarketPlaces/UploadFilesDialog?key=" + sKey + "&handler=" + oVendorInfo.ClientSide.LinkForm.UploadFilesHandler + '&modelkey=' + sModelKey

        @uploadFileDlg = @$el.find('div#upload-files-form').dialog(
            height: 600,
            width: 600,
            modal: true,
            title: 'Upload VAT Return Files'
            resizable: false
            dialogClass: 'upload-files-dialog'
            closeOnEscape: false
        )

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
