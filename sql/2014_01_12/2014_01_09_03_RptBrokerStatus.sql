IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptBrokerStatus]') AND type in (N'P', N'PC'))
	EXECUTE('CREATE PROCEDURE [dbo].[RptBrokerStatus] AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptBrokerStatus
@DateStart DATETIME,
@DateEnd   DATETIME,
@CustomerID INT = NULL,
@CustomerNameOrEmail NVARCHAR(256) = NULL
AS
BEGIN
	SELECT
		max(CR.Id) CrmId,
		CR.CustomerId
	INTO
		#CRMNotes
	FROM
		CustomerRelations CR
		left JOIN CashRequests O
			ON O.IdCustomer = CR.CustomerId
			--AND O.UnderwriterDecision = 'Approved'
		left JOIN CRMStatuses sts ON CR.StatusId = sts.Id
		INNER JOIN Customer C ON CR.CustomerId = C.Id
	WHERE
		C.IsTest = 0
		AND
		(@CustomerID IS NULL OR @CustomerID = CR.CustomerId)
		AND
		(
			@CustomerNameOrEmail IS NULL
			OR
			@CustomerID IS NOT NULL
			OR
			(
				C.Name LIKE '%' + @CustomerNameOrEmail + '%'
				OR
				C.FullName LIKE '%' + @CustomerNameOrEmail + '%'
			)
		)
	GROUP BY
		CR.CustomerId

	------------------------------------------------------------------------------

	SELECT
		CR.CustomerId,
		CR.UserName,
		sts.Name,
		CR.Comment
	INTO
		#CRMFinal
	FROM
		CustomerRelations CR
		INNER JOIN #CRMNotes N ON CR.Id = N.CrmId
		INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id

	------------------------------------------------------------------------------

	SELECT 
		C.Id,
		C.Name AS Email,
		C.FullName,
		O.UnderwriterDecision,
		O.UnderwriterDecisionDate,
		O.ManagerApprovedSum,
		O.UnderwriterComment,
		L.LoanAmount,
		CR.Name AS CRMStatus,
		CR.Comment,
		CASE 
     		WHEN C.IsOffline = 1 THEN 'Offline'
            ELSE 'Online'
  		END AS SegmentType
	FROM
		Customer C
		left JOIN CashRequests O
			ON C.Id = O.IdCustomer
			AND O.UnderwriterDecision = 'Approved'
		LEFT JOIN Loan L
			ON O.Id = L.RequestCashId
		LEFT JOIN #CRMFinal CR ON CR.CustomerId = O.IdCustomer
	WHERE
		C.IsTest = 0
		AND
		C.ReferenceSource LIKE 'liqcen'
		AND 
		(@CustomerID IS NULL OR @CustomerID = C.Id)
		AND
		(
			@CustomerNameOrEmail IS NULL
			OR
			@CustomerID IS NOT NULL
			OR
			(
				C.Name LIKE '%' + @CustomerNameOrEmail + '%'
				OR
				C.FullName LIKE '%' + @CustomerNameOrEmail + '%'
			)
		)
	ORDER BY
		O.CreationDate DESC
	
	------------------------------------------------------------------------------

	DROP TABLE #CRMNotes
	DROP TABLE #CRMFinal
END

GO

IF NOT EXISTS (SELECT 1 FROM ReportScheduler WHERE Type = 'RPT_BROKER_STATUS') 
BEGIN
	INSERT INTO dbo.ReportScheduler (
		Type, Title, StoredProcedure,
		IsDaily, IsWeekly, IsMonthly,
		Header,
		Fields,
		ToEmail, IsMonthToDate
	)
	VALUES (
		'RPT_BROKER_STATUS', 'Broker Status', 'RptBrokerStatus',
		1, 0, 0,
		'Id,Email,Full Name,Underwriter Decision,Underwriter Decision Date,Manager Approved Sum,Underwriter Comment,Loan Amount,CRM Status,Comment,Segment Type',
	    '!Id,Email,FullName,UnderwriterDecision,UnderwriterDecisionDate,ManagerApprovedSum,UnderwriterComment,LoanAmount,CRMStatus,Comment,SegmentType',
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
		r.Type = 'RPT_BROKER_STATUS'
		AND NOT EXISTS (
			SELECT *
			FROM ReportsUsersMap
			WHERE UserID = u.Id
			AND ReportID = r.ID
		)
		
	DECLARE @ReportId INT  = (SELECT Id FROM ReportScheduler r WHERE r.Type = 'RPT_BROKER_STATUS')
	DECLARE @ReportArgId INT = (SELECT Id FROM ReportArgumentNames WHERE Name = 'DateRange')
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId) VALUES (@ReportArgId,@ReportId)
END
GO

