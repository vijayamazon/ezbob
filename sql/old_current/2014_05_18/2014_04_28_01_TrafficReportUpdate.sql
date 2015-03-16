UPDATE dbo.ReportScheduler
SET Header = 'Channel,Visits,Visitors,Registrations,% of registrations,Applications,Num of approved,Num of rejected,Loans,Loan Amount,Cost,New Customer Cost,ROI,Css'
	, Fields = 'Channel,Visits,Visitors,Registrations,%PercentOfRegistrations,Applications,NumOfRejected,NumOfRejected,Loans,LoanAmount,Cost,NewCustomerCost,ROI,{Css'
WHERE Type = 'RPT_TRAFFIC_REPORT'
GO

