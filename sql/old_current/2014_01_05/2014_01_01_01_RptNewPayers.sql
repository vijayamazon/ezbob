IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewPayers]') AND type in (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE [dbo].[RptNewPayers] AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptNewPayers
@DateStart DATE,
@DateEnd DATE
AS
	SELECT C.Id,C.Fullname,L.LoanAmount,L.[Date]  
	FROM Customer C, Loan L 
	WHERE C.IsTest = 0 
	AND C.Id = L.CustomerId 
	AND L.[Date] >= dateadd(d,-30,@DateEnd)
	AND L.[Date] < dateadd(d,-25,@DateEnd) 
	AND C.Id IN (SELECT CustomerId
			     FROM Loan 
			     GROUP BY CustomerId 
			     HAVING count(1) = 1)

GO


IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_NEW_PAYERS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_NEW_PAYERS', 'New Payers', 'RptNewPayers',
		0, 0, 0,
		'Id,Fullname,LoanAmount,Date',
		'Id,Fullname,LoanAmount,Date',
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
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya')
		AND
		r.Type = 'RPT_NEW_PAYERS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_NEW_PAYERS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

