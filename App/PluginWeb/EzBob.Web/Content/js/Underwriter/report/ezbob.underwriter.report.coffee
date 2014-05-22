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

    serializeData: ->
        reports: @model.toJSON()

    events:
        "change #reportsDdl" : "reportChanged"
        "click #getReportBtn" : "getReportClicked"
        "click #downloadReportBtn" : "downloadReportClicked"

    onRender: ->
         @ui.reportsDdl.chosen();
         @ui.datesDdl.chosen();

    reportChanged: ->
        console.log("report changed")

    downloadReportClicked: ->
        if(@ui.reportsDdl.val() == '0' or @ui.datesDdl.val() == '0')
            alertify.error 'Select report and/or date range'
            return false

        window.location = "#{window.gRootPath}Underwriter/Report/DownloadReport/?reportId=#{@ui.reportsDdl.val()}&reportDate=#{@ui.datesDdl.val()}" 

    getReportClicked: ->
        if(@ui.reportsDdl.val() == '0' or @ui.datesDdl.val() == '0')
            alertify.error 'Select report and/or date range'
            return false

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

    show: ->
        @$el.show()

    hide: ->
        @$el.hide()
