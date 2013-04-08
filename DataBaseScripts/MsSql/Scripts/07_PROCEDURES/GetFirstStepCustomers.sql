IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFirstStepCustomers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetFirstStepCustomers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetFirstStepCustomers
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
if OBJECT_ID('tempdb..#Shops') is not NULL
BEGIN
                DROP TABLE #Shops
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

SELECT x.eMail
FROM
(
 
SELECT
                   Customer.Id,
                   Customer.Name AS eMail,
                   Customer.GreetingMailSentDate AS DateRegister,
                   Customer.FirstName,
                   Customer.Surname SurName,
                   Customer.DaytimePhone,
                   Customer.MobilePhone,
                   #Shops.Shops,
                   #MaxOffer.MaxApproved,
                   #SumOfLoans.SumOfLoans,
                   T.AnualTurnover,
                   T.ExpirianRating,
                   T.MedalType,
                   CASE
                   WHEN T.MedalType='Silver' THEN ROUND(T.AnualTurnover*0.06,0)
                   WHEN T.MedalType='Gold' THEN ROUND(T.AnualTurnover*0.08,0)
                   WHEN T.MedalType='Platinum' THEN ROUND(T.AnualTurnover*0.10,0)
                   WHEN T.MedalType='Diamond' THEN ROUND(T.AnualTurnover*0.12,0)
                   END PhoneOffer
FROM   Customer
 
LEFT JOIN #Shops ON #Shops.CustomerId = Customer.Id
LEFT JOIN #MaxOffer ON #MaxOffer.CustomerId = Customer.Id
LEFT JOIN #SumOfLoans ON #SumOfLoans.CustomerId = Customer.Id
LEFT JOIN #AnualTurnover T ON T.CustomerId = Customer.Id
WHERE
                                Customer.IsTest = 0
                                AND Name NOT like '%ezbob%'
                                AND Name NOT LIKE '%liatvanir%'
                                AND Customer.GreetingMailSentDate >= @DateStart AND Customer.GreetingMailSentDate < @DateEnd
                               
 
GROUP BY Customer.Id,Customer.Name,Customer.FirstName,Customer.Surname,Customer.DaytimePhone,Customer.MobilePhone,
                                Customer.GreetingMailSentDate,#Shops.Shops,#MaxOffer.MaxApproved,#SumOfLoans.SumOfLoans,T.ExpirianRating,
                                T.AnualTurnover,T.MedalType
 ) x
 WHERE
 x.FirstName IS NULL AND x.SurName IS NULL AND x.Shops IS NULL
 
 END
GO
