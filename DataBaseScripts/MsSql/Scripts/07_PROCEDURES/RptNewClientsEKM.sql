IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewClientsEKM]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewClientsEKM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptNewClientsEKM
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
 
 
if OBJECT_ID('tempdb..#Shops') is not NULL
BEGIN
                DROP TABLE #Shops
END

if OBJECT_ID('tempdb..#EKMCLients') is not NULL
BEGIN
                DROP TABLE #EKMCLients 
END

if OBJECT_ID('tempdb..#EKMCLientsClean') is not NULL
BEGIN
                DROP TABLE #EKMCLientsClean 
END

if OBJECT_ID('tempdb..#MaxOffer') is not NULL
BEGIN
                DROP TABLE #MaxOffer
END
 
if OBJECT_ID('tempdb..#SumOfLoans') is not NULL
BEGIN
                DROP TABLE #SumOfLoans
END
 
if OBJECT_ID('tempdb..#tmp1') is not NULL
BEGIN
                DROP TABLE #tmp1
END
 
if OBJECT_ID('tempdb..#AnualTurnover') is not NULL
BEGIN
                DROP TABLE #AnualTurnover
END
 
---- # OF SHOPS PER CUSTOMER ----
 
SELECT CustomerId,count(MarketPlaceId) AS Shops
INTO #Shops
FROM MP_CustomerMarketPlace
GROUP BY CustomerId
 
---- MAX OFFER THAT WAS OFFERED TO CUSTOMER ----
 
SELECT IdCustomer AS CustomerId,max(ManagerApprovedSum) MaxApproved
INTO #MaxOffer
FROM CashRequests
GROUP BY IdCustomer
               
-- EKM CLients
SELECT 
	DISTINCT CustomerId 
INTO #EKMCLients
FROM 
	MP_CustomerMarketPlace S,
	MP_MarketplaceType T
WHERE 
	S.MarketPlaceId = T.Id AND
	T.Name = 'EKM'


INSERT INTO #EKMClients 
SELECT Id CustomerId FROM Customer WHERE ReferenceSource LIKE 'EKM%'	


SELECT DISTINCT CustomerId INTO #EKMCLientsClean FROM #EKMClients  

--SELECT Id FROM Customer WHERE Id IN (SELECT CustomerId FROM #EKMClientsClean)
--RETURN

--SELECT count(1) FROM #EKMClients
--SELECT count(1) FROM #EKMCLientsClean

---- SUM OF LOANS THAT CUST GOT -----

SELECT CustomerId,sum(LoanAmount) SumOfLoans
INTO #SumOfLoans
FROM Loan
GROUP BY CustomerId
 
----- CALC FOR ANUAL TURNOVER -----
SELECT IdCustomer AS CustomerId,max(CreationDate)AS CreationDate
INTO #tmp1
FROM CashRequests
GROUP BY IdCustomer
ORDER BY 1
 
SELECT A.CustomerId,A.creationdate,R.MedalType,R.AnualTurnover,R.ExpirianRating
INTO #AnualTurnover
FROM #tmp1 A
JOIN CashRequests R ON R.IdCustomer = A.CustomerId
WHERE R.CreationDate = A.CreationDate
 
--------------------------------------------------------------------------
SELECT
                   Customer.Id,
                   Customer.GreetingMailSentDate AS DateRegister,
                   #Shops.Shops,
                   #MaxOffer.MaxApproved,
                   #SumOfLoans.SumOfLoans,
                   Customer.ReferenceSource
FROM   
	Customer
LEFT JOIN #Shops ON #Shops.CustomerId = Customer.Id
LEFT JOIN #MaxOffer ON #MaxOffer.CustomerId = Customer.Id
LEFT JOIN #SumOfLoans ON #SumOfLoans.CustomerId = Customer.Id
LEFT JOIN #AnualTurnover T ON T.CustomerId = Customer.Id
WHERE
    Customer.IsTest = 0
    AND Name NOT like '%ezbob%'
    AND Name NOT LIKE '%liatvanir%'
    AND Customer.GreetingMailSentDate >= @DateStart AND Customer.GreetingMailSentDate < @DateEnd
    AND Customer.Id IN (SELECT CustomerId FROM #EKMCLientsClean)
ORDER BY SumOfLoans desc                            
   
END
GO
