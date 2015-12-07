var EzBob = EzBob || {};

EzBob.LoanScheduleView = Backbone.Marionette.ItemView.extend({
	template: '#loan-schedule-template',

	serializeData: function() {
		var data = {
			schedule: this.options.schedule.Schedule,
			setupFee: this.options.schedule.SetupFee,
			averageAnnualCostofBorrowing: this.options.schedule.RealInterestCost,
			total: this.options.schedule.Total,
			totalInterest: this.options.schedule.TotalInterest,
			totalPrincipal: this.options.schedule.TotalPrincipal,
			isShowGift: this.options.isShowGift,
			isShowExportBlock: this.options.isShowExportBlock,
			OfferedCreditLine: this.options.schedule.Details.OfferedCreditLine,
			RepaymentPeriod: this.options.schedule.Details.RepaymentPeriod,
			InterestRate: this.options.schedule.Details.InterestRate,
			LoanType: this.options.schedule.Details.LoanType,
			isShowExceedMaxInterestForSource: this.options.isShowExceedMaxInterestForSource,
			MaxInterestForSource: this.options.schedule.MaxInterestForSource,
			LoanSourceName: this.options.schedule.LoanSourceName,
			ManualAddressWarning: this.options.ManualAddressWarning,
			customer: this.options.customer,
			refNum: this.options.refNum,
			apr: this.options.schedule.Apr,
			isPersonal: this.options.isPersonal
			
		};

		if (data.MaxInterestForSource === null)
			data.MaxInterestForSource = -1;

		return data;
	}, // serializeData
}); // EzBob.LoanScheduleView

EzBob.LoanScheduleViewDlg = EzBob.LoanScheduleView.extend({
	events: {
		'click .pdf-link': 'exportToPdf',
		'click .excel-link': 'exportToExcel',
	}, // events

	exportToPdf: function(e) {
		var $el = $(e.currentTarget);
		return $el.attr('href', window.gRootPath + 'Underwriter/Schedule/Export?id=' + this.options.offerId + '&isExcel=false&isShowDetails=false&customerId=' + this.options.customerId);
	}, // exportToPdf

	exportToExcel: function(e) {
		var $el = $(e.currentTarget);
		return $el.attr('href', window.gRootPath + 'Underwriter/Schedule/Export?id=' + this.options.offerId + '&isExcel=true&isShowDetails=false&customerId=' + this.options.customerId);
	}, // exportToExcel


    
	jqoptions: function() {
		return {
			modal: true,
			width: 880
		};
	}, // jqoptions
}); // EzBob.LoanScheduleViewDlg

