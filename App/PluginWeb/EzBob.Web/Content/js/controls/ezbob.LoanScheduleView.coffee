root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.LoanScheduleView extends Backbone.Marionette.ItemView
    template: "#loan-schedule-template"

    serializeData: ->
        return {
            schedule: @options.schedule.Schedule
            apr: @options.schedule.Apr
            setupFee: @options.schedule.SetupFee
            realInterestCost: @options.schedule.RealInterestCost
            total: @options.schedule.Total
            totalInterest : @options.schedule.TotalInterest
            totalPrincipal : @options.schedule.TotalPrincipal
            isShowGift : @options.isShowGift
            isShowExportBlock:@options.isShowExportBlock
        }


class EzBob.LoanScheduleViewDlg extends EzBob.LoanScheduleView

    events:
        "click .pdf-link": "exportToPdf",
        "click .excel-link": "exportToExcel"

    exportToPdf: (e) ->
        $el = $(e.currentTarget);
        $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" +@options.offerId+"&isExcel=false&isShowDetails=false&customerId="+@options.customerId);
    
    exportToExcel: (e) ->
        $el = $(e.currentTarget); 
        $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" +@options.offerId+"&isExcel=true&isShowDetails=false&customerId="+@options.customerId);
    
    jqoptions: ->
        modal: true
        width: 600