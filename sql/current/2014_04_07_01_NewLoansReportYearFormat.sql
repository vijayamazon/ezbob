UPDATE ReportScheduler
SET Fields = 'CustomerId, LoanAmount, ReferenceSource, IsOffline, MonthPart, !YearPart, GoogleCookie, Date'
WHERE type_assembly_usages = 'RPT_NEW_LOANS'
GO
