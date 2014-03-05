IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptBrokerStatus]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptBrokerStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptBrokerStatus] 
	(@DateStart DATETIME,
@DateEnd   DATETIME,
@CustomerID INT = NULL,
@CustomerNameOrEmail NVARCHAR(256) = NULL)
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
