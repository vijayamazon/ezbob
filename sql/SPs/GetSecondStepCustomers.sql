IF OBJECT_ID('GetSecondStepCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE GetSecondStepCustomers AS SELECT 1')
GO

ALTER PROCEDURE GetSecondStepCustomers
@DateStart DATETIME,
@DateEnd DATETIME
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

	---- SumOfOrders ID ----
	SELECT Id 
	INTO #SumOfOrdersId
	FROM MP_AnalyisisFunction 
	WHERE Name = 'TotalSumOfOrders'
	
	
	---- # OF SHOPS PER CUSTOMER ----
	DECLARE @eBayId 	INT,
			@AmazonId 	INT,
			@PaypalId 	INT;
	
	SET @eBayId   = (SELECT Id FROM MP_MarketplaceType WHERE Id = 1 );
	SET @AmazonId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 2 );
	SET @PaypalId = (SELECT Id FROM MP_MarketplaceType WHERE Id = 3 );
	
	
	SELECT CustomerId,count(MarketPlaceId) AS Shops
	INTO #Shops
	FROM MP_CustomerMarketPlace
	WHERE MarketPlaceId IN (@eBayId,@AmazonId)
	GROUP BY CustomerId
	
	---- PAYPAL ACCOUNT PER CUSTOMER ----
	SELECT CustomerId,count(MarketPlaceId) AS Paypal
	INTO #Paypal
	FROM MP_CustomerMarketPlace
	WHERE MarketPlaceId IN (@PaypalId)
	GROUP BY CustomerId
	
	
	---- TEMP FOLDER WITH STORES ANUAL SALES ----
	DECLARE @SalesPeriod INT;
			
	SET @SalesPeriod = (SELECT Id FROM MP_AnalysisFunctionTimePeriod WHERE Id = 4 );
	
	SELECT A.CustomerMarketPlaceId,max(A.Updated) UpdatedDate
	INTO #MP_Stores
	FROM MP_AnalyisisFunctionValues A, #SumOfOrdersId B
	WHERE A.AnalyisisFunctionId = B.Id
		AND A.AnalysisFunctionTimePeriodId = @SalesPeriod
	GROUP BY A.CustomerMarketPlaceId
	
	
	---- TEMP FOLDER WITH STORES ANUAL SALES INC CUSTOMER ID ----
	SELECT A.CustomerMarketPlaceId,A.UpdatedDate,round(B.ValueFloat,1) AS AnualSales,C.CustomerId
	INTO #temp1
	FROM #MP_Stores A,
		 MP_AnalyisisFunctionValues B,
		 MP_CustomerMarketPlace C,
		 #SumOfOrdersId D
	WHERE 	A.CustomerMarketPlaceId = b.CustomerMarketPlaceId
			AND A.UpdatedDate=b.Updated
			AND A.CustomerMarketPlaceId = C.Id
		    AND B.AnalyisisFunctionId = D.Id
			AND B.AnalysisFunctionTimePeriodId = 4
	ORDER BY 1
	
	---- JOIN TEMP1 WITH CUSTOMER TABLE ----
	SELECT  C.Id,C.Name AS EmailAddress,C.FirstName,C.Surname,A.CustomerMarketPlaceId,A.UpdatedDate,A.AnualSales,C.WizardStep
	INTO #temp2
	FROM #temp1 A
	JOIN Customer C ON C.Id = A.CustomerId
	WHERE C.Name NOT like '%ezbob%' 
	  AND C.Name NOT LIKE '%liatvanir%'
	  AND C.istest!=1
	GROUP BY A.CustomerMarketPlaceId,A.UpdatedDate,A.AnualSales,C.Id,C.Name,
			 C.FirstName,C.Surname,C.WizardStep
	ORDER BY 1
	
	---- TEMP TABLE WITH CUSTOMER DETAILS ----
	SELECT C.Id,C.Name,C.GreetingMailSentDate,C.FirstName,C.Surname,C.WizardStep
	INTO #temp3
	FROM Customer C
	WHERE C.Name NOT like '%ezbob%' 
	  AND C.Name NOT LIKE '%liatvanir%'
	  AND C.istest!=1
	
	---- FINAL TABLE WITH CUSTID, STORES & ANUAL SALES ----
	SELECT A.Name AS eMail,
		   --A.Id AS CustomerId, C.Shops AS NumOfStores,  CASE WHEN D.Paypal >= 1 THEN 'Y' ELSE 'N' END AS HasPaypal, Sum(B.AnualSales) AS AnualSales,  Sum(B.AnualSales)*0.06 AS ApproximateLoanOfferNotRounded,
		   ROUND((Sum(B.AnualSales)*0.06)/100, 0) * 100 AS ApproximateLoanOffer
		   
	FROM #temp3 A
	LEFT JOIN #temp2 B ON A.Id = B.Id
	LEFT JOIN #Shops C ON C.CustomerId = A.Id
	LEFT JOIN #Paypal D ON D.CustomerId = A.Id
	WHERE A.GreetingMailSentDate >= @DateStart 
		  AND A.GreetingMailSentDate < @DateEnd AND A.FirstName IS NULL AND A.Surname IS NULL AND B.AnualSales > 8000
	GROUP BY A.Id,C.Shops,D.Paypal, A.Name, A.FirstName, A.Surname

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

	------------------------------------------------------------------------------
	--
	-- The End.
	--
	------------------------------------------------------------------------------
END
GO
