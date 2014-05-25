root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.ReportModel extends Backbone.Model
    url: "#{gRootPath}Underwriter/Report/GetAll"

class EzBob.Underwriter.ReportView extends Backbone.Marionette.ItemView
    template: "#report-template"

    initialize: ->
        @model = new EzBob.Underwriter.ReportModel()
        @model.on "change reset", @render, @
        @model.fetch()

    ui:
        "reportsDdl" : "#reportsDdl"
        "datesDdl" : "#datesDdl"
        "reportArea" : "#reportDiv"
        "dateRange" : "#form-date-range"
        

    serializeData: ->
        reports: @model.toJSON()

    events:
        "change #reportsDdl" : "reportChanged"
        "change #datesDdl" : "dateChanged"
        "click #getReportBtn" : "getReportClicked"
        "click #downloadReportBtn" : "downloadReportClicked"

    onRender: ->
         @ui.reportsDdl.chosen()
         @ui.datesDdl.chosen()

    reportChanged: ->
        console.log(@ui.reportsDdl.val(), 'report')

    dateChanged: ->
        if(@ui.datesDdl.val() == 'Custom')
            @initDateRange()
        else
            @destroyDateRange()


    downloadReportClicked: ->
        if(@ui.reportsDdl.val() == '0' or @ui.datesDdl.val() == '0')
            alertify.error 'Select report and/or date range'
            return false
        
        if(@ui.datesDdl.val() == 'Custom')
            from = EzBob.formatDateTimeCS(@ui.dateRange.data('daterangepicker').startDate)
            to = EzBob.formatDateTimeCS(@ui.dateRange.data('daterangepicker').endDate)
            window.location = "#{window.gRootPath}Underwriter/Report/DownloadReportDates/?reportId=#{@ui.reportsDdl.val()}&from=#{from}&to=#{to}" 
        else
            window.location = "#{window.gRootPath}Underwriter/Report/DownloadReport/?reportId=#{@ui.reportsDdl.val()}&reportDate=#{@ui.datesDdl.val()}" 

    getReportClicked: ->
        if(@ui.reportsDdl.val() == '0' or @ui.datesDdl.val() == '0')
            alertify.error 'Select report and/or date range'
            return false

        if(@ui.datesDdl.val() == 'Custom')
            fromDate = EzBob.formatDateTimeCS(@ui.dateRange.data('daterangepicker').startDate)
            toDate = EzBob.formatDateTimeCS(@ui.dateRange.data('daterangepicker').endDate)
            xhr = $.post("#{window.gRootPath}Underwriter/Report/GetReportDates", 
                reportId : @ui.reportsDdl.val()
                from : fromDate
                to : toDate
            )
        else
            xhr = $.post("#{window.gRootPath}Underwriter/Report/GetReport", 
                reportId : @ui.reportsDdl.val()
                reportDate : @ui.datesDdl.val()
            )

        xhr.done (res) =>
            if res.report?
                @ui.reportArea.html(res.report)
                @formatTable(res.columns)

    formatTable: (columns)->
        $("#tableReportData").addClass "table table-bordered table-striped blue-header centered"
        oDataTableArgs =
          aLengthMenu: [
            [10,25,50,100,200,-1]
            [10,25,50,100,200,"All"]
          ]
          iDisplayLength: 100
          aaSorting: []
          aoColumns: columns

        $("#tableReportData").dataTable oDataTableArgs


    initDateRange: ->
        @ui.dateRange.show()
        @ui.dateRange.daterangepicker(
            format: "MM/dd/yyyy"
            startDate: Date.today().add(days: -29)
            endDate: Date.today()
            minDate: "01/01/2012"
            locale:
                applyLabel: "Select"
                fromLabel: "From"
                toLabel: "To&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
            showWeekNumbers: true
            buttonClasses: ['btn-success', 'btn-fullwidth']

        , (start, end) ->
            $("#form-date-range span").html start.toString("MMMM d, yyyy") + " - " + end.toString("MMMM d, yyyy")
            return
        )

        @$el.find("#form-date-range span").html Date.today().add(days: -29).toString("MMMM d, yyyy") + " - " + Date.today().toString("MMMM d, yyyy")

    destroyDateRange: ->
        @ui.dateRange.hide()
        #@ui.dateRange.data('daterangepicker').remove()
        return

    show: ->
        @$el.show()

    hide: ->
        @$el.hide()
