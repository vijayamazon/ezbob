root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.DashboardView extends Backbone.Marionette.ItemView
    template: "#dashboard-template"

    initialize: (options) ->
        @crmModel = options.crmModel
        @personalModel = options.personalModel
        @experianModel = options.experianModel
        @bindTo @model, "change sync", @render, this
        @bindTo @crmModel, "change sync", @render, this
        @bindTo @personalModel, "change sync", @render, this
        @bindTo @experianModel, "change sync", @render, this

    serializeData: ->
        m: @model.toJSON()
        crm: @crmModel.toJSON()
        experian: @experianModel.toJSON()

    events:
        'click a[data-action="collapse"]' : "boxToolClick"
        'click a[data-action="close"]' : "boxToolClick"
        
    boxToolClick: (e) ->
      obj = e.currentTarget
      false if $(obj).data("action") is `undefined`
      action = $(obj).data("action")
      btn = $(obj)
      switch action
        when "collapse"
          $(btn).children("i").addClass "anim-turn180"
          $(obj).parents(".box").children(".box-content").slideToggle 500, ->
            if $(this).is(":hidden")
              $(btn).children("i").attr "class", "fa fa-chevron-down"
            else
              $(btn).children("i").attr "class", "fa fa-chevron-up"
            return false

        when "close"
          $(obj).parents(".box").fadeOut 500, ->
            $(this).parent().remove()
            return false

        when "config"
          $("#" + $(obj).data("modal")).modal "show"
      false

    onRender: ->
        #console.log 'render', @model


