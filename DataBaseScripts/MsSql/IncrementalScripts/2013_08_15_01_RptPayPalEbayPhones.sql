
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptPaypalEbayPhones]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptPaypalEbayPhones]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE RptPaypalEbayPhones

		@DateStart DATETIME,
		@DateEnd   DATETIME
AS
BEGIN
	SET @DateStart = CONVERT(DATE, @DateStart)
	SET @DateEnd = CONVERT(DATE, @DateEnd)


if OBJECT_ID('tempdb..#temp1') is not NULL
BEGIN
                DROP TABLE #temp1
END
 
if OBJECT_ID('tempdb..#temp2') is not NULL
BEGIN
                DROP TABLE #temp2
END

if OBJECT_ID('tempdb..#MP1') is not NULL
BEGIN
                DROP TABLE #MP1
END

 
if OBJECT_ID('tempdb..#MP_temp1') is not NULL
BEGIN
                DROP TABLE #MP_temp1
END

----------- GET PHONE FROM CUSTOMERS WHO ADDED PAYPAL ---------

SELECT M.CustomerId,C.Name AS email,C.Fullname,C.GreetingMailSentDate AS SignUpDate,P.Phone
INTO #temp1
FROM MP_PayPalPersonalInfo P
JOIN MP_CustomerMarketPlace M ON M.Id = P.CustomerMarketPlaceId
JOIN Customer C ON C.Id = M.CustomerId
WHERE C.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
					OR C.IsTest=1)
	  AND C.Fullname IS NULL	

----------- GET PHONE FROM CUSTOMERS WHO ADDED EBAY  ---------

SELECT 	M3.CustomerId,
		C.Name AS email,
		C.Fullname,
		C.GreetingMailSentDate AS SignUpDate,
		M1.Phone
INTO #temp2
FROM MP_EbayUserAddressData M1,
	 MP_EbayUserData M2,
	 MP_CustomerMarketPlace M3
JOIN Customer C ON C.Id = M3.CustomerId
WHERE 	M1.Id = M2.RegistrationAddressId 
		AND M2.CustomerMarketPlaceId = M3.Id 
		AND C.Fullname IS NULL
		AND C.Id NOT IN 
				  ( SELECT C.Id
					FROM Customer C 
					WHERE Name LIKE '%ezbob%'
					OR Name LIKE '%liatvanir%'
					OR Name LIKE '%q@q%'
					OR Name LIKE '%1@1%'
					OR C.IsTest=1)
GROUP BY M3.CustomerId,
		 C.Name,
		 C.Fullname,
		 C.GreetingMailSentDate,
		 M1.Phone


-------------- EBAY & PAYPAL TURNOVER ------------------
----------- MARKET PLACE TYPE = 1 (EBAY) ---------------
----------- MARKET PLACE TYPE = 3 (PAYPAL) -------------

SELECT A.CustomerMarketPlaceId,max(A.Updated) UpdatedDate,
	   CASE 	
	   WHEN A.AnalyisisFunctionId = 7 THEN 'eBay'
	   WHEN A.AnalyisisFunctionId = 50 THEN 'PayPal'
	   END AS MarketPlaceName
INTO #MP1
FROM MP_AnalyisisFunctionValues A
WHERE A.AnalyisisFunctionId IN (7,50)  
	AND A.AnalysisFunctionTimePeriodId = 4
GROUP BY A.CustomerMarketPlaceId,
		CASE 	
	    WHEN A.AnalyisisFunctionId = 7 THEN 'eBay'
	    WHEN A.AnalyisisFunctionId = 50 THEN 'PayPal'
	    END


SELECT C.CustomerId,max(round(B.ValueFloat,1)) AS AnualSales,convert(DATE,B.Updated) AS LastUpdateDate
INTO #MP_temp1
FROM #MP1 A,
	 MP_AnalyisisFunctionValues B,
	 MP_CustomerMarketPlace C
WHERE 	A.CustomerMarketPlaceId = B.CustomerMarketPlaceId
		AND A.UpdatedDate = B.Updated
		AND A.CustomerMarketPlaceId = C.Id
	    AND B.AnalyisisFunctionId IN (7,50)
		AND B.AnalysisFunctionTimePeriodId = 4
GROUP BY C.CustomerId,convert(DATE,B.Updated)
HAVING max(round(B.ValueFloat,1)) >= 10000
ORDER BY 1


--------- FINAL TABLE - PAYPAL & EBAY PHONES ----------

SELECT T2.CustomerId,T2.email,T2.SignUpDate,max(MP.AnualSales) AS Turnover,T2.Phone AS eBayPhone,T1.Phone AS PayPalPhone
FROM #temp2 T2  
LEFT JOIN #temp1 T1 ON T1.CustomerId = T2.CustomerId
JOIN #MP_temp1 MP ON MP.CustomerId = T2.CustomerId
JOIN Customer C ON C.Id = T2.CustomerId
WHERE T2.CustomerId NOT IN (SELECT R.IdCustomer FROM CashRequests R)
	  AND T2.SignUpDate >= @DateStart
	  AND T2.SignUpDate <= @DateEnd
GROUP BY T2.CustomerId,T2.email,T2.SignUpDate,T2.Phone,T1.Phone,C.WizardStep


DROP TABLE #temp1
DROP TABLE #temp2
DROP TABLE #MP1
DROP TABLE #MP_temp1


END
GO

