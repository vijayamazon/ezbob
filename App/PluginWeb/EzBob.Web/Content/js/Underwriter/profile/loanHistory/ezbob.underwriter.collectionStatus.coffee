root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.CollectionStatusModel extends Backbone.Model
    urlRoot: -> "#{window.gRootPath}Underwriter/CollectionStatus/Index?Id=#{@get('customerId')}&currentStatus=#{@get('currentStatus')}"
                 
class EzBob.CollectionStatusItemsView extends Backbone.Marionette.ItemView
    template: '#collection-status-items-template' 

class EzBob.Underwriter.CollectionStatusLayout extends Backbone.Marionette.Layout
    template: '#collection-status-layout-template'
    
    initialize: () ->
        @modelBinder = new Backbone.ModelBinder()

    bindings:
        currentStatus:
            selector: "input[name='currentStatus']"
        customerId:
            selector:"input[name='customerId']"

    regions: 
        list: '#list-items'
        content: '#collection-view'

    events: 
        'change #collection-status-items' : 'changeStatus'
 
    changeStatus: ->
        currentStatus = $("#collection-status-items option:selected").val()
        @model.set "currentStatus": parseInt(currentStatus)
        @renderStatusValue()
        @

    renderStatusValue: =>
        currentStatus = @model.get "currentStatus"
        if currentStatus == 3 or currentStatus == 4
            @model.fetch()
            @$el.find('#collection-view').show()
            collectionStatusView = new EzBob.Underwriter.CollectionStatusView model:@model
            @content.show(collectionStatusView)
        else
            @$el.find('#collection-view').hide()
        false

    save : =>
        form = @$el.find('form')
        postData = form.serialize() 
        action = "#{window.gRootPath}Underwriter/CollectionStatus/Save/"
        $.post(action, postData )
        @trigger('saved')
        @close()
        false


    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        common = new EzBob.CollectionStatusItemsView 
        @list.show(common)
        @$el.find("#collection-status-items [value='#{@model.get('CurrentStatus')}']").attr("selected", "selected")
        @renderStatusValue()
        
        @

    jqoptions: ->
        modal: true
        resizable: false
        title: 'Customer Status'
        draggable: true
        width: "400"
        buttons:
            "OK": @onSave
            "Cancel": @onCancel
    
    onCancel: =>
        @close()
    
    onSave: =>
        @save()

class EzBob.Underwriter.CollectionStatusView extends Backbone.Marionette.Layout
    template: '#collection-status-template'

    initialize: () ->
        @modelBinder = new Backbone.ModelBinder()
    
    events: 
        'change .isAddCollectionFee' : 'readOnlyCollectionFee'
        "click .uploadFiles" : "upload"

    bindings:
        CollectionDateOfDeclaration:
            selector: "input[name='collectionDateOfDeclaration']"
            converter: EzBob.BidingConverters.dateTime
        CollectionDescription:
            selector: "#collectionDescription"
     
    readOnlyCollectionFee: (e) ->
        number = e.currentTarget.getAttribute('data-items-num')
        isChecked = $("input[name='items[#{number}].isAddCollectionFee']").prop("checked")
        @$el.find("input[name='items[#{number}].isAddCollectionFee']").attr("value", isChecked)
        @$el.find("input[name='items[#{number}].collectionFee']").attr('readonly', !isChecked)

    upload: () ->
        $("#addNewDoc").click()
        false

    showDateOfDeclaration :=>
        currentStatus = @model.get "currentStatus"
        if currentStatus == 3
            @$el.find('#date').show()
        if currentStatus == 4
            @$el.find('#date').hide()

    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        @$el.find('#collectionDateOfDeclaration').datepicker({ autoclose: true, format: 'dd/mm/yyyy' })
        @$el.parents('.ui-dialog').find("button").addClass 'btn-back'
        @$el.find('.collectionFee').autoNumeric({ 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F',  mDec: '2', vMax: '999999999999999' };)
        @showDateOfDeclaration()
        @

