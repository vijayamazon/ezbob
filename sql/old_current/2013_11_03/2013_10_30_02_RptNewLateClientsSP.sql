IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewLateClients]') AND type in (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE [dbo].[RptNewLateClients] AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptNewLateClients
@DateStart DATE,
@DateEnd DATE
AS
	SELECT
		C.Id,
		C.Name,
		C.FirstName,
		C.Surname,
		AmountDue
	FROM
		LoanSchedule S
		INNER JOIN Loan L ON S.LoanId = L.Id
		INNER JOIN Customer C ON L.CustomerId = C.Id AND C.IsTest = 0
	WHERE
		S.Date >= @DateStart AND S.Date < @DateEnd
		AND
		S.Status IN ('StillToPay', 'Late')
		AND
		C.CollectionStatus = 0
GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_NEW_LATE_CLIENTS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_NEW_LATE_CLIENTS', 'New Late Clients', 'RptNewLateClients',
		1, 0, 0,
		'Id,Name,FirstName,Surname,AmountDue',
		'Id,Name,FirstName,Surname,AmountDue',
		'nimrodk@ezbob.com', 0
	)

	INSERT INTO ReportsUsersMap (UserID, ReportID)
	SELECT
		u.Id,
		r.Id
	FROM
		ReportUsers u,
		ReportScheduler r
	WHERE
		u.UserName IN ('alexbo', 'stasd', 'nimrodk')
		AND
		r.Type = 'RPT_NEW_LATE_CLIENTS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
END
GO
