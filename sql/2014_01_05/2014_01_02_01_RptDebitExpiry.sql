IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDebitExpiry]') AND type in (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE [dbo].[RptDebitExpiry] AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptDebitExpiry
@DateStart DATE,
@DateEnd DATE
AS
	SELECT C.Id,C.Fullname,C.name,C.MobilePhone,C.DaytimePhone,P.CardNo,P.ExpireDateString,p.CardHolder
	FROM PayPointCard P, Customer C 
	WHERE C.Id = P.CustomerId 
	AND ExpireDate >= getdate() 
	AND ExpireDate < dateadd(m,1,getdate()) 
	AND C.Id IN (SELECT CustomerId 
				 FROM Loan 
				 WHERE Loan.Status <> 'PaidOff')

GO


IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_DEBIT_EXPIRY') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_DEBIT_EXPIRY', 'Debit Expiry', 'RptDebitExpiry',
		0, 0, 0,
		'Id,Fullname,name,MobilePhone,DaytimePhone,CardNo,ExpireDateString,CardHolder',
		'Id,Fullname,name,MobilePhone,DaytimePhone,CardNo,ExpireDateString,CardHolder',
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
		u.UserName IN ('alexbo', 'stasd', 'nimrodk', 'eilaya', 'emanuellea')
		AND
		r.Type = 'RPT_DEBIT_EXPIRY'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_DEBIT_EXPIRY')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

