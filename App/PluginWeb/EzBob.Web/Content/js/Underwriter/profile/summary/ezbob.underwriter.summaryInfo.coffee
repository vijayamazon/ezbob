root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}


class EzBob.Underwriter.SummaryInfoView extends Backbone.Marionette.ItemView
    template: "#profile-summary-template"
    initialize: ->
        @bindTo @model, "change", @render, this

    events:
        "keydown #CommentArea": "CommentAreaChanged"
        "click #commentSave": "saveComment"

    CommentAreaChanged: ->
        @CommentSave.removeClass "disabled"
        @CommentArea.css "border", "1px solid yellow"

    saveComment: ->
        that = this
        $.post(window.gRootPath + "Underwriter/Profile/SaveComment",
            Id: @model.get("Id")
            comment: @CommentArea.val()
        ).done(->
            that.CommentArea.css "border", "1px solid #ccc"
            that.CommentSave.addClass "disabled"
        ).fail (data) ->
            that.CommentArea.css "border", "1px solid red"
            console.error data.responseText
        false

    serializeData: ->
        m: @model.toJSON()

    onRender: ->
        @CommentSave = @$el.find("#commentSave")
        @CommentArea = @$el.find("#CommentArea")
        @$el.find('a[data-bug-type]').tooltip({title: 'Report bug'});
        #added new label into MPs tab
        isNew = @model.get("MarketPlaces").IsNew
        $("#new-ribbon-marketplaces").toggle Convert.toBool isNew

class EzBob.Underwriter.SummaryInfoModel extends Backbone.Model
    idAttribute: "Id"
    urlRoot: window.gRootPath + "Underwriter/Profile/Index"