IF OBJECT_ID('RptDidntFinishApp') IS NULL
	EXECUTE('CREATE PROCEDURE RptDidntFinishApp AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptDidntFinishApp
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		C.Id AS CustomerId,
		C.Fullname,
		C.GreetingMailSentDate AS SignUpDate,
		C.Name AS EmailAddress,
		W.WizardStepTypeDescription AS WizardStep,
		C.DaytimePhone,
		C.MobilePhone,
		PersonalScore = CONVERT(INT, NULL),
		NumOfDefaults = CONVERT(INT, NULL),
		CompanyScore = CONVERT(INT, NULL),
		ThinFile = CONVERT(BIT, NULL)
	INTO
		#temp1
	FROM
		Customer C
		INNER JOIN WizardStepTypes W ON W.WizardStepTypeID = C.WizardStep
	WHERE
		C.Id NOT IN (
			SELECT C.Id
			FROM Customer C 
			WHERE Name LIKE '%ezbob%'
			OR Name LIKE '%liatvanir%'
			OR Name LIKE '%q@q%'
			OR Name LIKE '%1@1%'
			OR C.IsTest=1
		)
		AND
		C.WizardStep != 4
		AND
		C.BrokerID IS NULL
		AND
		@DateStart <= C.GreetingMailSentDate AND C.GreetingMailSentDate <= @DateEnd

	------------------------------------------------------------------------------
	--
	-- Set analytics data
	--
	------------------------------------------------------------------------------

	DECLARE @CustomerID INT

	DECLARE cur CURSOR FOR SELECT CustomerId FROM #temp1
	OPEN cur

	FETCH NEXT FROM cur INTO @CustomerID

	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE #temp1 SET
			PersonalScore = p.Score,
			NumOfDefaults = p.NumOfDefaults,
			ThinFile      = p.ThinFile,
			CompanyScore  = c.Score
		FROM
			#temp1 t,
			dbo.udfGetCustomerPersonalAnalytics(@CustomerID, NULL) p,
			dbo.udfGetCustomerCompanyAnalytics(@CustomerID, NULL, 0, 0, 0) c
		WHERE
			t.CustomerId = @CustomerID

		FETCH NEXT FROM cur INTO @CustomerID
	END

	CLOSE cur
	DEALLOCATE cur

	------------------------------------------------------------------------------
	--
	-- GET LATEST CRM NOTES
	--
	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------
	--
	-- Output
	--
	------------------------------------------------------------------------------

	SELECT
		T1.CustomerId,
		T1.Fullname,
		T1.SignUpDate,
		T1.EmailAddress,
		T1.WizardStep,
		T1.DaytimePhone,
		T1.MobilePhone,
		T1.PersonalScore,
		T1.NumOfDefaults,
		T1.CompanyScore,
		T1.ThinFile,
		N.NoteDate AS CRMNoteDate,
		N.UserName AS CRMUsername,
		N.Comment AS CRMComment,
		N.CRMStatus,
		N.CRMAction
	FROM
		#temp1 T1
		LEFT JOIN #CRMNotes N ON N.CustomerId = T1.CustomerId
	WHERE
		(T1.PersonalScore IS NULL OR T1.PersonalScore >= 550)
	  	AND
	  	(T1.CompanyScore IS NULL OR T1.CompanyScore >= 15)
	  	AND
	  	(T1.NumOfDefaults IS NULL OR T1.NumOfDefaults <= 2)

	------------------------------------------------------------------------------
	--
	-- Cleanup
	--
	------------------------------------------------------------------------------

	DROP TABLE #CRMNotes
	DROP TABLE #MaxNoteDate
	DROP TABLE #temp1
END
GO
