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
	-- Main request
	--
	--------------------------------------------------------------------------

	SELECT
		Customer.Name AS eMail
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
	-- Drop temp tables.
	--
	--------------------------------------------------------------------------

	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	--------------------------------------------------------------------------
	--
	-- The End.
	--
	--------------------------------------------------------------------------
END
GO
