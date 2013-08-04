IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCustAnualSales]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCustAnualSales]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptCustAnualSales
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
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

	---- SumOfOrders ID ----
	SELECT
		Id
	INTO
		#SumOfOrdersId
	FROM
		MP_AnalyisisFunction
	WHERE
		Name = 'TotalSumOfOrders'


	---- # OF SHOPS PER CUSTOMER ----
	DECLARE
		@eBayId   INT,
		@AmazonId INT,
		@PaypalId INT

	SET @eBayId   = (SELECT Id FROM MP_MarketplaceType WHERE Id = 1)
	SET @AmazonId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 2)
	SET @PaypalId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 3)

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

	---- TEMP FOLDER WITH STORES ANUAL SALES ----
	DECLARE @SalesPeriod INT

	SET @SalesPeriod = (SELECT Id FROM MP_AnalysisFunctionTimePeriod WHERE Id = 4);

	SELECT
		A.CustomerMarketPlaceId,
		MAX(A.Updated) UpdatedDate
	INTO
		#MP_Stores
	FROM
		MP_AnalyisisFunctionValues A
		INNER JOIN #SumOfOrdersId B ON A.AnalyisisFunctionId = B.Id
	WHERE
		A.AnalysisFunctionTimePeriodId = @SalesPeriod
	GROUP BY
		A.CustomerMarketPlaceId

	---- TEMP FOLDER WITH STORES ANUAL SALES INC CUSTOMER ID ----
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

	---- JOIN TEMP1 WITH CUSTOMER TABLE ----
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
		JOIN Customer C ON C.Id = A.CustomerId
	WHERE
		C.Name NOT like '%ezbob%' 
		AND
		C.Name NOT LIKE '%liatvanir%'
		AND
		C.Name NOT LIKE '%@test%'
		AND
		C.IsTest = 0
	GROUP BY
		A.CustomerMarketPlaceId,
		A.UpdatedDate,
		A.AnualSales,
		C.Id,C.Name,
		C.FirstName,
		C.Surname,
		C.WizardStep
	ORDER BY
		1

	---- TEMP TABLE WITH CUSTOMER DETAILS ----
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
	WHERE
		C.Name NOT like '%ezbob%' 
		AND 
		C.Name NOT LIKE '%liatvanir%'
		AND
		C.Name NOT LIKE '%@test%'
		AND
		C.IsTest = 0

	---- FINAL TABLE WITH CUSTID, STORES & ANUAL SALES ----
	SELECT
		A.Id AS CustomerId,
		A.WizardStep,	
		C.Shops AS NumOfStores,
		CASE
			WHEN D.Paypal >= 1 THEN 'Y'
			ELSE 'N'
		END AS HasPaypal,
		SUM(B.AnualSales) AS AnualSales,
		ROUND((SUM(B.AnualSales) * 0.06), -2) AS OfferAmount
	FROM
		#temp3 A
		LEFT JOIN #temp2 B ON A.Id = B.Id
		LEFT JOIN #Shops C ON C.CustomerId = A.Id
		LEFT JOIN #Paypal D ON D.CustomerId = A.Id
	WHERE
		CONVERT(DATE, @DateStart) <= A.GreetingMailSentDate AND A.GreetingMailSentDate < CONVERT(DATE, @DateEnd)
	GROUP BY
		A.Id,
		A.WizardStep,
		C.Shops,
		D.Paypal

	DROP TABLE #SumOfOrdersId
	DROP TABLE #Shops
	DROP TABLE #Paypal
	DROP TABLE #MP_Stores
	DROP TABLE #temp1
	DROP TABLE #temp2
	DROP TABLE #temp3
END
GO
