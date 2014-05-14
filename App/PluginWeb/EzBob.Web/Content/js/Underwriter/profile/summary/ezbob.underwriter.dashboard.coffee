﻿root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.DashboardView extends Backbone.Marionette.ItemView
    template: "#dashboard-template"

    initialize: (options) ->
        @crmModel = options.crmModel
        @personalModel = options.personalModel
        @experianModel = options.experianModel
        @propertiesModel = options.propertiesModel
        @mpsModel = options.mpsModel
        @loanModel = options.loanModel

        @bindTo @model, "change sync", @render, this
        @bindTo @crmModel, "change sync", @render, this
        @bindTo @personalModel, "change sync", @personalModelChanged, this
        @bindTo @experianModel, "change sync", @render, this
        @bindTo @propertiesModel, "change sync", @render, this
        @bindTo @mpsModel, "change sync", @render, this
        @bindTo @loanModel, "change sync", @render, this

    serializeData: ->
        m: @model.toJSON()
        crm: _.first(_.filter(@crmModel.toJSON(), (crm) -> crm.User isnt 'System'), 5) #taking first 5 non System CRMs
        experian: @experianModel.toJSON()
        properties: @propertiesModel.toJSON()
        mps: @mpsModel.toJSON()
        loan: @loanModel.toJSON()
        affordability: null
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

    personalModelChanged: (e, a)->
        if(e and a and @model)
            @model.fetch()

    onRender: ->
        if (@model.get('Alerts') != undefined)
            if (@model.get('Alerts').length == 0) 
                $('#customer-label-span').removeClass('label-warning').removeClass('label-important').addClass('label-success')
            else
                if(_.some(@model.get('Alerts'), (alert) -> return alert.AlertType is 'danger'))
                    $('#customer-label-span').removeClass('label-success').removeClass('label-warning').addClass('label-important')
                else
                    $('#customer-label-span').removeClass('label-success').removeClass('label-important').addClass('label-warning')

        if(@experianModel && @experianModel.get('ConsumerHistory'))
            historyScoresSorted = _.sortBy(@experianModel.get('ConsumerHistory'), (history) ->
                return history.Date)
            consumerHistoryScores = _.pluck(historyScoresSorted, 'Score').join(',')
            @$el.find(".consumerScoreGraph").attr('values',consumerHistoryScores)

        if(@experianModel && @experianModel.get('CompanyHistory'))
            historyScoresSorted = _.sortBy(@experianModel.get('CompanyHistory'), (history) ->
                return history.Date)
            companyHistoryScores = _.pluck(historyScoresSorted, 'Score').join(',')
            @$el.find(".companyScoreGraph").attr('values', companyHistoryScores)
        
        $(".inline-sparkline").sparkline("html",
            width: "100%"
            height: "100%"
            lineWidth: 2
            spotRadius: 3
            lineColor: "#88bbc8"
            fillColor: "#f2f7f9"
            spotColor: "#14ae48"
            maxSpotColor: "#e72828"
            minSpotColor: "#f7941d"
            chartRangeMin: -1
            valueSpots: { ':549' : 'red', '550:749' : 'yellow', '750:' : 'green' }
        )




