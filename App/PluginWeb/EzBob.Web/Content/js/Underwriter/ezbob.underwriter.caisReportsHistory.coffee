root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.CaisReportHistoryModel extends Backbone.Model
    idAttribute: "Id"
    url: -> "#{window.gRootPath}Underwriter/CaisReportsHistory/Index"

class EzBob.Underwriter.CaisReportHistoryView  extends Backbone.Marionette.ItemView
    template: '#cais-report-history-template'

