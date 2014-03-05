IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewClientsEKM]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewClientsEKM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewClientsEKM] 
	(@DateStart DATETIME,
@DateEnd   DATETIME)
AS
BEGIN
	IF OBJECT_ID('tempdb..#Shops') is not NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#EKMCLients') is not NULL
		DROP TABLE #EKMCLients

	IF OBJECT_ID('tempdb..#MaxOffer') is not NULL
		DROP TABLE #MaxOffer

	if OBJECT_ID('tempdb..#SumOfLoans') is not NULL
		DROP TABLE #SumOfLoans

	if OBJECT_ID('tempdb..#tmp1') is not NULL
		DROP TABLE #tmp1

	if OBJECT_ID('tempdb..#AnualTurnover') is not NULL
		DROP TABLE #AnualTurnover

	---- # OF SHOPS PER CUSTOMER ----

	SELECT
		CustomerId,
		COUNT(MarketPlaceId) AS Shops
	INTO
		#Shops
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	---- MAX OFFER THAT WAS OFFERED TO CUSTOMER ----

	SELECT
		IdCustomer AS CustomerId,
		MAX(ManagerApprovedSum) MaxApproved
	INTO
		#MaxOffer
	FROM
		CashRequests
	GROUP BY
		IdCustomer

	-- EKM CLients

	CREATE TABLE #EKMClients (CustomerId INT)

	INSERT INTO #EKMClients
	SELECT DISTINCT CustomerId
	FROM MP_CustomerMarketPlace S
		INNER JOIN MP_MarketplaceType T ON S.MarketPlaceId = T.Id AND T.Name = 'EKM'
	UNION
	SELECT Id
	FROM Customer
	WHERE ReferenceSource LIKE 'EKM%'

	---- SUM OF LOANS THAT CUST GOT -----

	SELECT
		CustomerId,
		SUM(LoanAmount) SumOfLoans
	INTO
		#SumOfLoans
	FROM
		Loan
	GROUP BY
		CustomerId

	----- CALC FOR ANUAL TURNOVER -----

	SELECT
		IdCustomer AS CustomerId,
		MAX(CreationDate) AS CreationDate
	INTO
		#tmp1
	FROM
		CashRequests
	GROUP BY
		IdCustomer
	ORDER BY
		1

	SELECT
		A.CustomerId,
		A.creationdate,
		R.MedalType,
		R.AnualTurnover,
		R.ExpirianRating
	INTO
		#AnualTurnover
	FROM
		#tmp1 A
		JOIN CashRequests R
			ON R.IdCustomer = A.CustomerId
			AND R.CreationDate = A.CreationDate

	SELECT
		Customer.Id,
		Customer.GreetingMailSentDate AS DateRegister,
		#Shops.Shops,
		#MaxOffer.MaxApproved,
		#SumOfLoans.SumOfLoans,
		Customer.ReferenceSource
	FROM
		Customer
		INNER JOIN #EKMClients ek ON Customer.Id = ek.CustomerId
		LEFT JOIN #Shops ON #Shops.CustomerId = Customer.Id
		LEFT JOIN #MaxOffer ON #MaxOffer.CustomerId = Customer.Id
		LEFT JOIN #SumOfLoans ON #SumOfLoans.CustomerId = Customer.Id
		LEFT JOIN #AnualTurnover T ON T.CustomerId = Customer.Id
	WHERE
		Customer.IsTest = 0
		AND
		Name NOT like '%ezbob%'
		AND
		Name NOT LIKE '%liatvanir%'
		AND
		CONVERT(DATE, @DateStart) <= Customer.GreetingMailSentDate AND Customer.GreetingMailSentDate < CONVERT(DATE, @DateEnd)
	ORDER BY
		SumOfLoans desc

	DROP TABLE #Shops
	DROP TABLE #EKMCLients
	DROP TABLE #MaxOffer
	DROP TABLE #SumOfLoans
	DROP TABLE #tmp1
	DROP TABLE #AnualTurnover
END
GO
