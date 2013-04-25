IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewClientsStep1]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewClientsStep1]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptNewClientsStep1
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN

if OBJECT_ID('tempdb..#tmp1') is not NULL 
BEGIN
	DROP TABLE #tmp1
END

if OBJECT_ID('tempdb..#tmp2') is not NULL 
BEGIN
	DROP TABLE #tmp2
END

if OBJECT_ID('tempdb..#tmpOffer') is not NULL 
BEGIN
	DROP TABLE #tmpOffer
END

SELECT 
	CustomerId,
	count(MarketPlaceId) Shops
INTO #tmp2
FROM 
	MP_CustomerMarketPlace
GROUP BY  
	CustomerId

SELECT 
	IdCustomer CustomerId,
	max(ManagerApprovedSum) MaxApproved
INTO #tmpOffer
FROM
	CashRequests
GROUP BY
	IdCustomer
	
SELECT 
	Name,
	datepart(dw,GreetingMailSentDate) DayOfWeek,
	GreetingMailSentDate DateRegister,
	ReferenceSource,
	#tmp2.Shops,
	Payment = CASE 
		WHEN Customer.AccountNumber IS NULL THEN 'NO'
		WHEN Customer.AccountNumber IS NOT NULL THEN 'YES'
	END,
	Personal = CASE
		WHEN Customer.FirstName IS NULL THEN 'NO'
		WHEN Customer.FirstName IS NOT NULL THEN 'YES'
	END,
	Complete = CASE
		WHEN Customer.ApplyForLoan IS NULL THEN 'NO'
		WHEN Customer.ApplyForLoan IS NOT NULL THEN 'YES'
	END,
	Customer.IsSuccessfullyRegistered,
	Customer.Status,
	Customer.MedalType,
	#tmpOffer.MaxApproved,
	Loan.LoanAmount,
	Customer.CreditSum CreditLeft,
	Loan.[Date],
	datepart(dw,Loan.[Date]) LoanDayOfWeek
	--FirstName,
	--SurName,
	
INTO #tmp1	
FROM 
	Customer 
	LEFT JOIN #tmp2 ON Customer.Id = #tmp2.CustomerId 
	LEFT JOIN Loan ON loan.CustomerId = Customer.Id 
	LEFT JOIN #tmpOffer ON #tmpOffer.CustomerId = Customer.Id
WHERE 	
	GreetingMailSentDate >= @DateStart AND
	GreetingMailSentDate <  @DateEnd  AND 
	IsTest = 0

SELECT Name,DateRegister, Shops, Payment,Personal, Complete FROM #tmp1 WHERE Name NOT like '%ezbob%' AND Name NOT LIKE '%q.q%' AND Name NOT LIKE '%liatvanir%' AND Shops IS NULL AND Status = 'Registered'

END
GO
