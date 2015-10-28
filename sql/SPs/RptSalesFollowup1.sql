IF OBJECT_ID('RptSalesFollowup1') IS NULL
	EXECUTE ('CREATE PROCEDURE RptSalesFollowup1 AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptSalesFollowup1
@DateStart DATE,
@DateEnd DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id,
		c.FullName,
		s.Description AS ResidentialStatus,
		c.TypeOfBusiness,
		c.DaytimePhone,
		c.MobilePhone,
		c.OverallTurnOver,
		c.Name,
		c.GreetingMailSentDate,
		c.ExperianConsumerScore AS ConsumerScore,
		CompanyScore = CONVERT(INT, NULL)
	INTO
		#output
	FROM
		Customer c
		LEFT JOIN CustomerPropertyStatuses s ON c.PropertyStatusId = s.Id
		LEFT JOIN CustomerRelations crm ON crm.CustomerId = c.Id
	WHERE
		c.WizardStep = 6 
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd 
		AND
		c.IsTest = 0 
	GROUP BY
		c.Id,
		c.FullName, 
		s.Description,
		c.TypeOfBusiness,
		c.DaytimePhone,
		c.MobilePhone,
		c.OverallTurnOver,
		c.Name,
		c.GreetingMailSentDate,
		c.ExperianConsumerScore
	HAVING
		COUNT(crm.Id) <= 3

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
			CompanyScore = c.Score
		FROM
			#output t,
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
		ResidentialStatus,
		TypeOfBusiness,
		DaytimePhone,
		MobilePhone,
		OverallTurnOver,
		Name,
		GreetingMailSentDate,
		ConsumerScore,
		CompanyScore
	FROM
		#output
	ORDER BY
		OverallTurnOver DESC
END
GO
