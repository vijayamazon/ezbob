IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptSaleStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptSaleStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
