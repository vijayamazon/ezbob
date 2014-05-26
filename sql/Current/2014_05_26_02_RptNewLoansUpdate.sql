UPDATE dbo.ReportScheduler
SET Header = 'Customer Id,Loan Amount,SourceRef,Google Cookie, Month'
	, Fields = '#CustomerId,LoanAmount,SourceRef,GoogleCookie,MonthPart'
WHERE Type = 'RPT_NEW_LOANS' 
GO

