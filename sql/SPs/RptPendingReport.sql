
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
		PersonalScore = CONVERT(INT, NULL),
		CompanyScore = CONVERT(INT, NULL),
		NumOfDefaults = CONVERT(INT, NULL),
		IsBroker = CASE WHEN C.BrokerID IS NOT NULL THEN 'BROKER ' + b.ContactName ELSE '' end,
		CR.Name AS CRMStatus,
		CR.Comment,
		CASE 
			WHEN C.IsOffline = 1 THEN 'Offline'
			ELSE 'Online'
		END AS SegmentType
	INTO
		#output
	FROM
		Customer C
		LEFT JOIN #CRMFinal CR ON CR.CustomerId = C.Id
		LEFT JOIN Broker b ON b.BrokerID = C.BrokerID
	WHERE
		C.CreditResult = 'ApprovedPending' 
		AND
		C.IsTest = 0

	------------------------------------------------------------------------------
	--
	-- Set analytics data
	--
	------------------------------------------------------------------------------

	DECLARE @CustomerID INT

	DECLARE cur CURSOR FOR SELECT Id FROM #output
	OPEN cur

	FETCH NEXT FROM cur INTO @CustomerID

	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE #output SET
			PersonalScore = p.Score,
			NumOfDefaults = p.NumOfDefaults,
			CompanyScore  = c.Score
		FROM
			#output t,
			dbo.udfGetCustomerPersonalAnalytics(@CustomerID, NULL) p,
			dbo.udfGetCustomerCompanyAnalytics(@CustomerID, NULL, 0, 0, 0) c
		WHERE
			t.Id = @CustomerID

		FETCH NEXT FROM cur INTO @CustomerID
	END

	CLOSE cur
	DEALLOCATE cur

	------------------------------------------------------------------------------

	SELECT
		Id,
		FullName,
		GreetingMailSentDate,
		ApplyForLoan,
		PendingStatus,
		PersonalScore,
		CompanyScore,
		NumOfDefaults,
		IsBroker,
		CRMStatus,
		Comment,
		SegmentType
	FROM
		#output

	------------------------------------------------------------------------------

	DROP TABLE #output
	DROP TABLE #CRMNotes
	DROP TABLE #CRMFinal
END
GO
