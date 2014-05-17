UPDATE ReportScheduler
SET Fields = 'CustomerId, LoanAmount, ReferenceSource, IsOffline, MonthPart, !YearPart, GoogleCookie, Date'
WHERE Type = 'RPT_NEW_LOANS'
GO
