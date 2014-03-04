root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CompanyFilesAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/CompanyFilesMarketPlaces/Accounts"

class EzBob.CompanyFilesAccounts extends Backbone.Collection
    model: EzBob.CompanyFilesAccountModel
    url: "#{window.gRootPath}Customer/CompanyFilesMarketPlaces/Accounts"


class EzBob.CompanyFilesAccountInfoView extends Backbone.Marionette.ItemView
    events:
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'
        'click a.hmrcBack': 'back'
        'click a.connect-account': 'connect'
        
    initialize: (options) ->
        @accountType = 'CompanyFiles'
        @template = '#' + @accountType + 'AccountInfoTemplate'
        @isOldInternetExplorer = 'Microsoft Internet Explorer' == navigator.appName && navigator.appVersion.indexOf("MSIE 1") == -1

    render: ->
        super()
        that = this
        @

    inputChanged: ->
        
    uploadFiles: ->
        

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        EzBob.App.trigger 'clear'
        'Upload Company Files'

    connect: ->
        @inputChanged()
        @trigger('completed');
        @trigger 'back'

        false



