root = exports ? this
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
        @companyModel = options.companyModel
        
        @bindTo @model, "change sync", @render, this
        @bindTo @crmModel, "change sync", @render, this
        @bindTo @personalModel, "change sync", @personalModelChanged, this
        @bindTo @experianModel, "change sync", @render, this
        @bindTo @companyModel, "change sync", @render, this
        @bindTo @propertiesModel, "change sync", @render, this
        @bindTo @mpsModel, "change sync", @render, this
        @bindTo @loanModel, "change sync", @render, this

        @expCompany = []
        
    serializeData: ->
        @expCompany = []
        if(@companyModel.get('DashboardModel'))
            @expCompany.push(@companyModel.get('DashboardModel'))
        if(@companyModel.get('Owners'))
            _.each(@companyModel.get('Owners'), (owner) =>
                @expCompany.push(owner.DashboardModel)
            )

        m: @model.toJSON()
        crm: _.first(_.filter(@crmModel.get('CustomerRelations'), (crm) -> crm.User isnt 'System'), 5) #taking first 5 non System CRMs
        experian: @experianModel.toJSON()
        company: @expCompany
        properties: @propertiesModel.toJSON()
        mps: @mpsModel.toJSON()
        loan: @loanModel.toJSON()
        affordability: _.first(_.filter(@mpsModel.toJSON(), (mp) -> mp.Name is 'HMRC'), 1)

    events:
        'click a[data-action="collapse"]' : "boxToolClick"
        'click a[data-action="close"]' : "boxToolClick"
        'click a[href^="#companyExperian"]': "companyChanged"
        
    companyChanged: (e) ->
        obj = e.currentTarget
        @$el.find('.company-name').text($(obj).data('companyname') + ' ' + $(obj).data('companyref'))

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
        @$el.find('a[data-bug-type]').tooltip({title: 'Report bug'})
        
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
            
            consumerHistoryCIIs = _.pluck(historyScoresSorted, 'CII').join(',')
            @$el.find(".consumerCIIGraph").attr('values',consumerHistoryCIIs)

        if(@experianModel && @experianModel.get('CompanyHistory'))
            historyScoresSorted = _.sortBy(@experianModel.get('CompanyHistory'), (history) ->
                return history.Date)
            companyHistoryScores = _.pluck(historyScoresSorted, 'Score').join(',')
            @$el.find(".companyScoreGraph0").attr('values', companyHistoryScores)
        
        @$el.find(".inline-sparkline").sparkline("html",
            width: "100%"
            height: "100%"
            lineWidth: 2
            spotRadius: 3.5
            lineColor: "#cfcfcf"
            fillColor: "transparent"
            spotColor: "#cfcfcf"
            maxSpotColor: "#cfcfcf"
            minSpotColor: "#cfcfcf"
            valueSpots: { ':' : '#cfcfcf' }
        )

        properties = @propertiesModel.toJSON()
        
        if(properties && properties.NetWorth)
            @drawDonut(@$el.find("#assets-donut"), "#00ab5d", properties.NetWorth / (properties.NetWorth + properties.SumOfMortgages))

        @$el.find('[data-toggle="tooltip"]').tooltip({'placement': 'bottom'});
        
        cc = @$el.find("#consumerScoreCanvas")
        @halfDonut(cc, cc.data('color'), cc.data('percent'))
        cii = @$el.find("#consumerCIICanvas")
        @halfDonut(cii, cii.data('color'), cii.data('percent'))
        
        if(@expCompany && @expCompany.length > 0 && !@expCompany[0].Error)
            _.each(@expCompany, (c, i) =>
                compC = @$el.find("#companyScoreCanvas" + i)
                @halfDonut(compC, compC.data('color'), compC.data('percent'))
                if(c.IsLimited)
                    profit = _.pluck(c.FinDataHistories, 'AdjustedProfit').reverse().join(',')
                    @$el.find("#companyProfit" + i).attr('values',profit)
                
                    equity = _.pluck(c.FinDataHistories, 'TangibleEquity').reverse().join(',')
                    @$el.find("#companyEquity" + i).attr('values',equity)
            )
        
        @$el.find('.bar-sparkline').sparkline("html",
            type:'bar'
            barColor:'#cfcfcf'
            height: "50px"
        )
        
        if(@experianModel and @experianModel.get('directorsModels'))
            directors = @experianModel.get('directorsModels').length;
            i = 0
            while i < directors
                cc = @$el.find("#directorScoreCanvas" + i)
                @halfDonut(cc, cc.data('color'), cc.data('percent'))
                cii = @$el.find("#directorCIICanvas" + i)
                @halfDonut(cii, cii.data('color'), cii.data('percent'))
                i++

    halfDonut: (el, fillColor, fillPercent) ->
        canvas = el[0];
        context = canvas.getContext('2d');
        x = canvas.width / 2;
        y = canvas.height / 2;
        radius = 40;
        startAngle = 1 * Math.PI;
        endAngle = 2 * Math.PI;
        counterClockwise = false;
        lineWidth = 15;
        context.beginPath();
        context.arc(x, y, radius, startAngle, endAngle, counterClockwise);
        context.lineWidth = lineWidth;
        context.strokeStyle = '#ebebeb';
        context.stroke();
        context.beginPath();
        context.arc(x, y, radius, startAngle, Math.PI*(1+fillPercent), counterClockwise);
        context.strokeStyle = fillColor;
        context.stroke();
        
    drawDonut:  (el, fillColor, fillPercent) ->
      canvas = el[0];
      context = canvas.getContext("2d")
      x = canvas.width / 2
      y = canvas.height / 2
      radius = 70
      startAngle = 1 * Math.PI
      endAngle = 4 * Math.PI
      lineWidth = 25
      context.beginPath()
      context.arc x, y, radius, startAngle, endAngle, false
      context.lineWidth = lineWidth
      context.strokeStyle = "#ebebeb"
      context.stroke()
      context.beginPath()
      context.arc x, y, radius, startAngle, Math.PI * (1 + fillPercent * 2), false
      context.strokeStyle = fillColor
      context.stroke()
      return
        
    




