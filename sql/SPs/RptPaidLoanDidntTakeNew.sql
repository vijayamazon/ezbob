IF OBJECT_ID('RptPaidLoanDidntTakeNew') IS NULL
	EXECUTE('CREATE PROCEDURE RptPaidLoanDidntTakeNew AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptPaidLoanDidntTakeNew
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON

	CREATE TABLE #temp1	(
		CustomerId INT NOT NULL,
		SegmentType VARCHAR(20)
	)

	------------------ INSERT INTO TEMP TABLE CUSTOMERS WHO PAID OFF THEIR LOANS ----------------

	INSERT INTO #temp1
	SELECT
		L.CustomerId,'Fully Repaid'
	FROM
		Loan L
		INNER JOIN Customer c ON c.Id = l.CustomerId
	WHERE
		L.CustomerId NOT IN (
			SELECT C.Id
			FROM Customer C 
			WHERE Name LIKE '%ezbob%'
			OR Name LIKE '%liatvanir%'
			OR Name LIKE '%q@q%'
			OR Name LIKE '%1@1%'
			OR C.IsTest = 1
		)
		AND
		C.CollectionStatus = 0
	GROUP BY L.CustomerId
	HAVING
		COUNT(L.Id) = COUNT(L.DateClosed)

	-------------- INSERT INTO TEMP TABLE CUSTOMERS WHO PAID MORE THAN 60% OF LOAN ---------------

	SELECT
		L.CustomerId,
		MAX(L.[Date]) AS LastLoanDate	
	INTO
		#temp2
	FROM
		Loan L
		INNER JOIN Customer c ON c.Id = l.CustomerId
	WHERE
		L.CustomerId NOT IN (
			SELECT C.Id
			FROM Customer C 
			WHERE Name LIKE '%ezbob%'
			OR Name LIKE '%liatvanir%'
			OR Name LIKE '%q@q%'
			OR Name LIKE '%1@1%'
			OR C.IsTest = 1
		)
		AND
		C.CollectionStatus = 0
		AND
		L.CustomerId NOT IN (SELECT T.CustomerId FROM #temp1 T)
	GROUP BY
		L.CustomerId	   	 
	HAVING
		COUNT(L.Id) - COUNT(L.DateClosed) = 1

	INSERT INTO #temp1
	SELECT
		T.CustomerId,
		'Paid More than 50%'
	FROM
		#temp2 T
		INNER JOIN Loan L ON L.CustomerId = T.CustomerId
	WHERE
		L.[Date] = T.LastLoanDate
		AND
		L.Principal < L.LoanAmount * 0.5

	--------------- GET LATEST CRM NOTES --------------

	SELECT
		R.CustomerId,
		MAX(R.Timestamp) AS NoteDate
	INTO
		#MaxNoteDate
	FROM
		CustomerRelations R
		INNER JOIN #temp1 T ON T.CustomerId = R.CustomerId
	GROUP BY
		R.CustomerId

	SELECT
		N.CustomerId,
		N.NoteDate,	
		R.Comment,
		R.UserName,
		S.Name AS CRMStatus,
		A.Name AS CRMAction
	INTO
		#CRMNotes
	FROM
		#MaxNoteDate N
		INNER JOIN CustomerRelations R ON R.CustomerId = N.CustomerId
		INNER JOIN CRMStatuses S ON S.Id = R.StatusId
		INNER JOIN CRMActions A ON A.Id = R.ActionId
	WHERE
		R.Timestamp = N.NoteDate

	---------------------- FINAL TABLE ---------------------

	SELECT
		T.CustomerId,
		C.Name AS EmailAddress,
		C.Fullname,
		T.SegmentType,
		CASE 
			WHEN C.CollectionStatus = 0 THEN 'Enabled'
			WHEN C.CollectionStatus = 1 THEN 'Disabled'
			WHEN C.CollectionStatus = 2 THEN 'Fraud'
			WHEN C.CollectionStatus = 3 THEN 'Legal'
			WHEN C.CollectionStatus = 4 THEN 'Default'
			WHEN C.CollectionStatus = 5 THEN 'Fraud Suspect'
			WHEN C.CollectionStatus = 6 THEN 'Risky'
			WHEN C.CollectionStatus = 7 THEN 'Bad'
			WHEN C.CollectionStatus = 8 THEN 'WriteOff'
			WHEN C.CollectionStatus = 9 THEN 'Debt Management'
		END AS CustomerStatus,
		COUNT(L.Id) AS NumOfLoansTook,
		PersonalScore = (SELECT Score FROM dbo.udfGetCustomerPersonalAnalytics(T.CustomerId, NULL)),
		MAX(L.DateClosed) AS DatePaidLastLoan,
		C.DaytimePhone,
		C.MobilePhone,
		N.NoteDate AS CRMNoteDate,
		N.UserName AS CRMUsername,
		N.Comment AS CRMComment,
		N.CRMStatus,
		N.CRMAction,
		CASE
			WHEN C.BrokerID IS NOT NULL THEN 'BrokerClient'   
			ELSE 'NonBroker'
		END AS BrokerOrNot
	FROM
		#temp1 T
		INNER JOIN Loan L ON L.CustomerId = T.CustomerId
		INNER JOIN Customer C ON C.Id = T.CustomerId
		LEFT JOIN #CRMNotes N ON N.CustomerId = T.CustomerId
	GROUP BY
		T.CustomerId,
		C.Name,
		C.Fullname,
		T.SegmentType,
		C.CollectionStatus,
		C.DaytimePhone,
		C.MobilePhone,
		N.NoteDate,
		N.UserName,
		N.Comment,
		N.CRMStatus,
		N.CRMAction,
		C.BrokerID

	DROP TABLE #temp1
	DROP TABLE #temp2
	DROP TABLE #MaxNoteDate
	DROP TABLE #CRMNotes
END
GO
