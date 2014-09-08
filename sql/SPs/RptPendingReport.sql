
IF OBJECT_ID('RptPendingReport') IS NULL
	EXECUTE('CREATE PROCEDURE RptPendingReport AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptPendingReport
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		MAX(CR.Id) CrmId,
		CR.CustomerId
	INTO
		#CRMNotes
	FROM
		CustomerRelations CR
		INNER JOIN CashRequests O
			ON O.IdCustomer = CR.CustomerId
			AND O.UnderwriterDecision = 'ApprovedPending'
		INNER JOIN CRMStatuses sts ON CR.StatusId = sts.Id
		INNER JOIN Customer C ON CR.CustomerId = C.Id
	WHERE
		C.IsTest = 0
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
		C.FullName,
		C.GreetingMailSentDate,
		C.ApplyForLoan,
		C.PendingStatus,
		A.PersonalScore,
		A.CompanyScore,
		A.NumOfDefaults,
		IsBroker = CASE WHEN C.BrokerID IS NOT NULL THEN 'BROKER ' + b.ContactName ELSE '' end,
		CR.Name AS CRMStatus,
		CR.Comment,
		CASE 
			WHEN C.IsOffline = 1 THEN 'Offline'
			ELSE 'Online'
		END AS SegmentType
	FROM
		CustomerAnalytics A
		INNER JOIN Customer C
			ON A.CustomerID = C.Id
			AND C.CreditResult = 'ApprovedPending' 
			AND C.IsTest = 0
		LEFT JOIN #CRMFinal CR ON CR.CustomerId = C.Id
		LEFT JOIN Broker b ON b.BrokerID = C.BrokerID

	------------------------------------------------------------------------------

	DROP TABLE #CRMNotes
	DROP TABLE #CRMFinal
END
GO
