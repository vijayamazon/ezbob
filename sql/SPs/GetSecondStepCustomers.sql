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

	IF OBJECT_ID('tempdb..#SumOfOrdersId') IS NOT NULL
		DROP TABLE #SumOfOrdersId

	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#Paypal') IS NOT NULL
		DROP TABLE #Paypal

	IF OBJECT_ID('tempdb..#MP_Stores') IS NOT NULL
		DROP TABLE #MP_Stores

	IF OBJECT_ID('tempdb..#temp1') IS NOT NULL
		DROP TABLE #temp1

	IF OBJECT_ID('tempdb..#temp2') IS NOT NULL
		DROP TABLE #temp2

	IF OBJECT_ID('tempdb..#temp3') IS NOT NULL
		DROP TABLE #temp3

	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

	------------------------------------------------------------------------------
	--
	-- SumOfOrders ID
	--
	------------------------------------------------------------------------------

	SELECT
		Id 
	INTO
		#SumOfOrdersId
	FROM
		MP_AnalyisisFunction 
	WHERE
		Name = 'TotalSumOfOrders'

	------------------------------------------------------------------------------
	--
	-- # OF SHOPS PER CUSTOMER
	--
	------------------------------------------------------------------------------

	DECLARE
		@eBayId INT,
		@AmazonId INT,
		@PaypalId INT

	SELECT @eBayId   = Id FROM MP_MarketplaceType WHERE InternalId = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	SELECT @AmazonId = Id FROM MP_MarketplaceType WHERE InternalId = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	SELECT @PaypalId = Id FROM MP_MarketplaceType WHERE InternalId = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'

	------------------------------------------------------------------------------

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Shops
	INTO
		#Shops
	FROM
		MP_CustomerMarketPlace
	WHERE
		MarketPlaceId IN (@eBayId, @AmazonId)
	GROUP BY
		CustomerId

	---- PAYPAL ACCOUNT PER CUSTOMER ----

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Paypal
	INTO
		#Paypal
	FROM
		MP_CustomerMarketPlace
	WHERE
		MarketPlaceId IN (@PaypalId)
	GROUP BY
		CustomerId

	------------------------------------------------------------------------------
	--
	-- TEMP FOLDER WITH STORES ANUAL SALES
	--
	------------------------------------------------------------------------------

	DECLARE @SalesPeriod INT

	SELECT @SalesPeriod = Id FROM MP_AnalysisFunctionTimePeriod WHERE InternalId = '1F9E6CEF-7251-4E1C-AC35-801265E732CD'

	------------------------------------------------------------------------------

	SELECT
		A.CustomerMarketPlaceId,
		MAX(A.Updated) AS UpdatedDate
	INTO
		#MP_Stores
	FROM
		MP_AnalyisisFunctionValues A
		INNER JOIN #SumOfOrdersId B ON A.AnalyisisFunctionId = B.Id
	WHERE
		A.AnalysisFunctionTimePeriodId = @SalesPeriod
	GROUP BY
		A.CustomerMarketPlaceId

	------------------------------------------------------------------------------
	--
	-- TEMP FOLDER WITH STORES ANUAL SALES INC CUSTOMER ID
	--
	------------------------------------------------------------------------------

	SELECT
		A.CustomerMarketPlaceId,
		A.UpdatedDate,
		ROUND(B.ValueFloat, 1) AS AnualSales,
		C.CustomerId
	INTO
		#temp1
	FROM
		#MP_Stores A
		INNER JOIN MP_AnalyisisFunctionValues B
			ON A.CustomerMarketPlaceId = b.CustomerMarketPlaceId
			AND A.UpdatedDate = b.Updated
		INNER JOIN MP_CustomerMarketPlace C ON A.CustomerMarketPlaceId = C.Id
		INNER JOIN #SumOfOrdersId D ON B.AnalyisisFunctionId = D.Id
	WHERE
		B.AnalysisFunctionTimePeriodId = 4
	ORDER BY
		1

	------------------------------------------------------------------------------
	--
	-- JOIN TEMP1 WITH CUSTOMER TABLE
	--
	------------------------------------------------------------------------------

	SELECT
		C.Id,
		C.Name AS EmailAddress,
		C.FirstName,
		C.Surname,
		A.CustomerMarketPlaceId,
		A.UpdatedDate,
		A.AnualSales,
		C.WizardStep
	INTO
		#temp2
	FROM
		#temp1 A
		INNER JOIN Customer C ON C.Id = A.CustomerId
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
	GROUP BY
		A.CustomerMarketPlaceId,
		A.UpdatedDate,
		A.AnualSales,
		C.Id,
		C.Name,
		C.FirstName,
		C.Surname,
		C.WizardStep
	ORDER BY
		1

	------------------------------------------------------------------------------
	--
	-- TEMP TABLE WITH CUSTOMER DETAILS
	--
	------------------------------------------------------------------------------

	SELECT
		C.Id,
		C.Name,
		C.GreetingMailSentDate,
		C.FirstName,
		C.Surname,
		C.WizardStep
	INTO
		#temp3
	FROM
		Customer C 
	LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = C.OriginID 	
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
		 

	------------------------------------------------------------------------------
	--
	-- FINAL TABLE WITH CUSTID, STORES & ANUAL SALES
	--
	------------------------------------------------------------------------------

	SELECT
		A.Id AS CustomerID,
		A.Name AS eMail,
		-- A.Id AS CustomerId,
		-- C.Shops AS NumOfStores,
		-- CASE WHEN D.Paypal >= 1 THEN 'Y' ELSE 'N' END AS HasPaypal,
		-- SUM(B.AnualSales) AS AnualSales,
		-- SUM(B.AnualSales) * 0.06 AS ApproximateLoanOfferNotRounded,
		ROUND((SUM(B.AnualSales) * 0.06) / 100, 0) * 100 AS ApproximateLoanOffer
	INTO
		#raw
	FROM
		#temp3 A
		LEFT JOIN #temp2 B ON A.Id = B.Id
		LEFT JOIN #Shops C ON C.CustomerId = A.Id
		LEFT JOIN #Paypal D ON D.CustomerId = A.Id
	WHERE
		 @DateStart <= A.GreetingMailSentDate AND A.GreetingMailSentDate < @DateEnd
		 AND
		 A.FirstName IS NULL
		 AND
		 A.Surname IS NULL
		 AND
		 B.AnualSales > 8000
	GROUP BY
		A.Id,
		C.Shops,
		D.Paypal,
		A.Name,
		A.FirstName,
		A.Surname

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
		r.ApproximateLoanOffer,
		b.BrokerEmail
	FROM
		#raw r
		LEFT JOIN dbo.udfBrokerEmailsForCustomerMarketing(@ids) b ON r.CustomerID = b.CustomerID

	------------------------------------------------------------------------------
	--
	-- Drop temp tables.
	--
	------------------------------------------------------------------------------

	IF OBJECT_ID('tempdb..#SumOfOrdersId') IS NOT NULL
		DROP TABLE #SumOfOrdersId

	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#Paypal') IS NOT NULL
		DROP TABLE #Paypal

	IF OBJECT_ID('tempdb..#MP_Stores') IS NOT NULL
		DROP TABLE #MP_Stores

	IF OBJECT_ID('tempdb..#temp1') IS NOT NULL
		DROP TABLE #temp1

	IF OBJECT_ID('tempdb..#temp2') IS NOT NULL
		DROP TABLE #temp2

	IF OBJECT_ID('tempdb..#temp3') IS NOT NULL
		DROP TABLE #temp3

	IF OBJECT_ID('tempdb..#raw') IS NOT NULL
		DROP TABLE #raw

	------------------------------------------------------------------------------
	--
	-- The End.
	--
	------------------------------------------------------------------------------
END
GO
