root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.LoanScheduleView extends Backbone.Marionette.ItemView
    template: "#loan-schedule-template"

    serializeData: ->
        return {
            schedule: @options.schedule.schedule
            apr: @options.schedule.apr
            setupFee: @options.schedule.setupFee
            realInterestCost: @options.schedule.realInterestCost
            total: @options.schedule.total
            totalInterest : @options.schedule.totalInterest
            totalPrincipal : @options.schedule.totalPrincipal
            isShowGift : @options.isShowGift
        }

class EzBob.LoanScheduleViewDlg extends EzBob.LoanScheduleView
    jqoptions: ->
        modal: true
        width: 600