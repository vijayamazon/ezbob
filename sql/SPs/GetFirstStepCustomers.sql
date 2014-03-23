IF OBJECT_ID('GetFirstStepCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE GetFirstStepCustomers AS SELECT 1')
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
	
	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

	--------------------------------------------------------------------------
	--
	-- # OF SHOPS PER CUSTOMER
	--
	--------------------------------------------------------------------------

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Shops
	INTO
		#Shops
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	--------------------------------------------------------------------------
	--
	-- Select relevant customers.
	--
	--------------------------------------------------------------------------

	SELECT
		Customer.Id AS CustomerID,
		Customer.Name AS eMail
	INTO
		#raw
	FROM
		Customer
		LEFT JOIN #Shops ON #Shops.CustomerId = Customer.Id
	WHERE
		(
			@IncludeTest = 1
			OR (
				Customer.IsTest = 0
				AND
				Customer.Name NOT like '%ezbob%'
			)
		)
		AND
		Customer.Name NOT LIKE '%liatvanir%'
		AND
		@DateStart <= Customer.GreetingMailSentDate AND Customer.GreetingMailSentDate < @DateEnd
		AND
		Customer.FirstName IS NULL
		AND
		Customer.SurName IS NULL
		AND
		#Shops.Shops IS NULL

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

	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

	--------------------------------------------------------------------------
	--
	-- The End.
	--
	--------------------------------------------------------------------------
END
GO
