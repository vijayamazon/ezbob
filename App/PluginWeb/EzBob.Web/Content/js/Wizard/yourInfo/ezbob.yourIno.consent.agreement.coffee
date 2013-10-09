root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.ConsentAgreementModel extends Backbone.Model
    defaults:
        id:0
        firstName:''
        middleInitial:''
        surname:''
        date: moment.utc().toDate();

class EzBob.ConsentAgreement extends Backbone.Marionette.ItemView
    template: "#consent-agreement-temlate"

    events:
        'click .print': 'onPrint'
        'click .download': 'onDownload'

    onDownload: ->
        id = @model.get ('id')
        firstName = @model.get ('firstName')
        middleInitial =  @model.get ('middleInitial')
        surname = @model.get ('surname')
        location.href = "#{window.gRootPath}Customer/Consent/Download?id=#{id}&firstName=#{firstName}&middleInitial=#{middleInitial}&surname=#{surname}"

    onPrint: -> 
        printElement("consent-conent")
