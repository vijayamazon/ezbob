IF OBJECT_ID('GetSecondStepCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE GetSecondStepCustomers AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetSecondStepCustomers
@DateStart DATETIME,
@DateEnd DATETIME,
@IncludeTest BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------
	--
	-- Drop temp tables (just in case).
	--
	------------------------------------------------------------------------------

	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

   	------------------------------------------------------------------------------
	--
	-- TEMP TABLE WITH CUSTOMER DETAILS
	--
	------------------------------------------------------------------------------

	SELECT
		C.Id CustomerID,
		C.Name eMail,
		C.GreetingMailSentDate,
		C.FirstName,
		C.Surname,
		C.WizardStep
	INTO
		#raw
	FROM
		Customer C 
	LEFT JOIN 
		CustomerOrigin o ON o.CustomerOriginID = C.OriginID 	
	WHERE
		(
			@IncludeTest = 1
			OR (
				C.Name NOT like '%ezbob%' 
				AND
				C.IsTest != 1
			)
		)
		AND
		C.Name NOT LIKE '%liatvanir%'
		AND o.Name='ezbob'
		AND @DateStart <= C.GreetingMailSentDate AND C.GreetingMailSentDate < @DateEnd
		AND C.WizardStep IN (5,6)

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

	------------------------------------------------------------------------------
	--
	-- Drop temp tables.
	--
	------------------------------------------------------------------------------

	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

	------------------------------------------------------------------------------
	--
	-- The End.
	--
	------------------------------------------------------------------------------
END

GO
