root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.HMRCAccountInfoView extends Backbone.Marionette.ItemView
    events:
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'
        'click a.hmrcBack': 'back'
        'click a.linkButton' : 'linkAccount'
        'click a.uploadButton' : 'uploadFiles'
        'click a.linkAccountBack': 'linkAccountBack'
        'click a.uploadFilesBack': 'uploadFilesBack'
        'click a.connect-account': 'connect'
        'click a.select-vat': 'selectVatFiles'

    initialize: (options) ->
        @uploadFilesDlg = null
        @accountType = 'HMRC'
        @template = '#' + @accountType + 'AccountInfoTemplate'
        @validator = undefined

    inputChanged: ->
        if (@validator == undefined)
            @validator = EzBob.validateHmrcLinkForm(this.$el.find('#hmrcLinkAccount'))
        enabled = EzBob.Validation.checkForm(EzBob.validateHmrcLinkForm(this.$el.find('#hmrcLinkAccount')))
        @$el.find('a.connect-account').toggleClass('disabled', !enabled)
        
    linkAccount: ->
        @$el.find('#linkAccountDiv').show()
        @$el.find('#initialDiv').hide()

    uploadFiles: ->
        console.log('dsfgdfgh')
        debugger
        @$el.find('#uploadFilesDiv').show()
        @$el.find('#initialDiv').hide()
        
    back: ->
        @trigger 'back'
        false

    linkAccountBack: ->
        @$el.find('#linkAccountDiv').hide()
        @$el.find('#initialDiv').show()
        false

    uploadFilesBack: ->
        @$el.find('#uploadFilesDiv').hide()
        @$el.find('#initialDiv').show()
        false

    getDocumentTitle: ->
        EzBob.App.trigger 'clear'
        'Link HMRC Account'

    connect: ->
        if (@validator == undefined)
            @validator = EzBob.validateHmrcLinkForm(this.$el.find('#hmrcLinkAccount'))
        if not EzBob.Validation.checkForm(@validator)
            @validator.form()
            return false

        return false if @$el.find('a.connect-account').hasClass('disabled')
        
        accountModel = @buildModel
        
        if not accountModel
            EzBob.App.trigger 'error', 'HMRC Account Data Validation Error'
            return false

        acc = new EzBob.CGAccountModel accountModel
        
        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', 'HMRC Account Saving Error'
            return false

        BlockUi 'on'
        xhr.always =>
            BlockUi 'off'

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            EzBob.App.trigger 'error', 'Failed to Save HMRC Account'

        xhr.done (res) =>
            if res.error
                EzBob.App.trigger 'error', res.error
                return false

            try
                @model.add(acc)

            EzBob.App.trigger 'info', 'HMRC Account Added Successfully'
            
            @$el.find('#hmrc_user_id').val("") 
            @$el.find('#hmrc_password').val("") 

            @inputChanged()
            @trigger('completed');
            @trigger 'back'

        false

    buildModel: ->
        accountModel = $.parseJSON $('div#cg-account-model-template').text()
        
        accountModel.accountTypeName = 'HMRC'
                    
        accountModel['login'] = @$el.find('#hmrc_user_id').val()
        accountModel['name'] = @$el.find('#hmrc_user_id').val()
        accountModel['password'] = @$el.find('#hmrc_password').val()

        delete accountModel.id
        console.log accountModel
        return accountModel

    selectVatFiles: (evt) ->
        evt.preventDefault()

        sKey = 'f' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000)
        sModelKey = 'model' + (new Date()).getTime() + 'x' + Math.floor(Math.random() * 1000000000)

        while window[sKey]
            sKey += Math.floor(Math.random() * 1000)

        while window[sModelKey]
            sModelKey += Math.floor(Math.random() * 1000)
            
        window[sModelKey] = =>
            return @buildModel

        window[sKey] = (sResult) =>
            delete window[sKey]
            delete window[sModelKey]

            @uploadFileDlg.dialog 'close'
            @uploadFileDlg = null

            oResult = JSON.parse sResult

            if oResult.error
                EzBob.App.trigger 'error', 'Problem Linking HMRC Account: ' + oResult.error.Data.error
            else
                if oResult.submitted
                    EzBob.App.trigger 'info', 'HMRC Account Added Successfully'

            @trigger 'completed'
            @trigger 'back'
        # end of sKey handler

        $('iframe', @$el.find('div#upload-files-form')).each (idx, iframe) ->
            iframe.setAttribute 'width', 570
            iframe.setAttribute 'height', 515
            iframe.setAttribute 'src', "#{window.gRootPath}Customer/CGMarketPlaces/UploadFilesDialog?key=" + sKey + "&handler=HandleUploadedHmrcVatReturn&modelkey=" + sModelKey
        # end of each

        @uploadFileDlg = @$el.find('div#upload-files-form').dialog(
            height: 600,
            width: 600,
            modal: true,
            title: 'Please upload the VAT returns'
            resizable: false
            dialogClass: 'upload-files-dialog'
            closeOnEscape: false
        )

        false

   