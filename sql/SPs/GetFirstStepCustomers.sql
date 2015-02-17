IF OBJECT_ID('GetFirstStepCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE GetFirstStepCustomers AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetFirstStepCustomers
@DateStart DATETIME,
@DateEnd DATETIME,
@IncludeTest BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	
	--------------------------------------------------------------------------
	--
	-- Drop temp tables (just in case).
	--
	--------------------------------------------------------------------------
	
	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

	--------------------------------------------------------------------------
	--
	-- Select relevant customers.
	--
	--------------------------------------------------------------------------

	SELECT
		c.Id AS CustomerID,
		c.Name AS eMail
	INTO
		#raw
	FROM
		Customer c
		LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
	WHERE
		(
			@IncludeTest = 1
			OR (c.IsTest = 0 AND c.Name NOT LIKE '%ezbob%')
		)
		AND
			c.Name NOT LIKE '%liatvanir%'
		AND
			@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
		AND
			c.WizardStep=1
		AND 
			c.IsAlibaba = 0
		AND 
			o.Name = 'ezbob'

	--------------------------------------------------------------------------
	--
	-- Find broker email (if relevant) and final select.
	--
	--------------------------------------------------------------------------

	DECLARE @ids IntList

	INSERT INTO @ids (Value)
	SELECT
		CustomerID
	FROM
		#raw

	--------------------------------------------------------------------------

	SELECT
		r.eMail,
		b.BrokerEmail
	FROM
		#raw r
		LEFT JOIN dbo.udfBrokerEmailsForCustomerMarketing(@ids) b ON r.CustomerID = b.CustomerID

	--------------------------------------------------------------------------
	--
	-- Drop temp tables.
	--
	--------------------------------------------------------------------------

	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

	--------------------------------------------------------------------------
	--
	-- The End.
	--
	--------------------------------------------------------------------------
END

GO


