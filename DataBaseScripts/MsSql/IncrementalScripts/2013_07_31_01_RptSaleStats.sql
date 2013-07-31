IF OBJECT_ID('RptSaleStats') IS NOT NULL
	DROP PROCEDURE RptSaleStats
GO

CREATE PROCEDURE RptSaleStats
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SET @DateEnd = CONVERT(DATE, @DateStart)
	SET @DateStart = DATEADD(week, -1, @DateEnd)
	
	SELECT
		max(CR.Id) CrmId,
		CR.CustomerId
	INTO
		#CRMNotes
	FROM
		CustomerRelations CR
		INNER JOIN CashRequests O
			ON O.IdCustomer = CR.CustomerId
			AND O.UnderwriterDecision = 'Approved'
		INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id
	WHERE
		@DateStart <= O.CreationDate AND O.CreationDate < @DateEnd
	GROUP BY
		CR.CustomerId

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
		CR.Comment
	FROM
		Customer C
		INNER JOIN CashRequests O
			ON C.Id = O.IdCustomer
			AND O.UnderwriterDecision = 'Approved'
		LEFT JOIN Loan L
			ON O.Id = L.RequestCashId
			AND (
				L.LoanAmount < O.ManagerApprovedSum
				OR
				L.LoanAmount IS NULL
			)
		LEFT JOIN #CRMFinal CR ON CR.CustomerId = O.IdCustomer
	WHERE
		@DateStart <= O.CreationDate AND O.CreationDate < @DateEnd
	ORDER BY
		O.CreationDate DESC
	
	DROP TABLE #CRMNotes
	DROP TABLE #CRMFinal
END
GO

IF EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_SALE_STATS')
	UPDATE ReportScheduler SET
		Title = 'Sale Stats',
		StoredProcedure = 'RptSaleStats',
		Header = 'Id,Email,Full Name,Underwriter Decision,Underwriter Decision Date,Manager Approved Sum,Underwriter Comment,Loan Amount,CRM Status,Comment',
		Fields = '!Id,Email,FullName,UnderwriterDecision,UnderwriterDecisionDate,ManagerApprovedSum,UnderwriterComment,LoanAmount,CRMStatus,Comment'
	WHERE
		Type = 'RPT_SALE_STATS'
ELSE
	INSERT INTO ReportScheduler (
		Type, Title, StoredProcedure, IsDaily, IsWeekly, IsMonthly,
		Header, Fields,
		ToEmail,
		IsMonthToDate
	)
	VALUES (
		'RPT_SALE_STATS', 'Sale Stats', 'RptSaleStats', 0, 0, 0,
		'Id,Email,Full Name,Underwriter Decision,Underwriter Decision Date,Manager Approved Sum,Underwriter Comment,Loan Amount,CRM Status,Comment',
		'!Id,Email,FullName,UnderwriterDecision,UnderwriterDecisionDate,ManagerApprovedSum,UnderwriterComment,LoanAmount,CRMStatus,Comment',
		'nimrodk@ezbob.com,alexbo+rpt@ezbob.com',
		0
	)
GO

