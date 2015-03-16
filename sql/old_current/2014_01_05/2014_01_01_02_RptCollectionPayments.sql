IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCollectionPayments]') AND type in (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE [dbo].[RptCollectionPayments] AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptCollectionPayments
@DateStart DATE,
@DateEnd DATE
AS
	SELECT C.Id,C.Fullname,T.Amount,T.PostDate,L.LoanAmount,L.Balance
	FROM LoanTransaction T, Loan L, Customer C 
	WHERE C.IsTest = 0 
	AND C.Id = L.CustomerId 
	AND L.Id = T.LoanId 
	AND (C.CollectionStatus IN (3,4,6,7)  OR C.CciMark = 1)
	AND T.PostDate >= @DateStart
	AND T.PostDate <= @DateEnd
	AND T.Status = 'Done' 
	AND T.Type = 'PaypointTransaction'
GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_COLLECTION_PAYMENTS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_COLLECTION_PAYMENTS', 'Collection Payments', 'RptCollectionPayments',
		0, 0, 0,
		'Id,Fullname,Amount,PostDate,LoanAmount,Balance',
		'Id,Fullname,Amount,PostDate,LoanAmount,Balance',
		'', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'emanuellea')
		AND
		r.Type = 'RPT_COLLECTION_PAYMENTS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_COLLECTION_PAYMENTS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

