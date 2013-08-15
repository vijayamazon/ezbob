IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewClientsFullEx]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewClientsFullEx]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptNewClientsFullEx
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#Shops') IS NOT NULL
		DROP TABLE #Shops

	IF OBJECT_ID('tempdb..#MaxOffer') IS NOT NULL
		DROP TABLE #MaxOffer

	IF OBJECT_ID('tempdb..#SumOfLoans') IS NOT NULL
		DROP TABLE #SumOfLoans

	IF OBJECT_ID('tempdb..#tmp1') IS NOT NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#AnualTurnover') IS NOT NULL
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
		JOIN CashRequests R ON R.IdCustomer = A.CustomerId
	WHERE
		R.CreationDate = A.CreationDate

	SELECT
		Customer.Id,
		Customer.Name AS eMail,
		Customer.GreetingMailSentDate AS DateRegister,
		Customer.FirstName,
		Customer.Surname SurName,
		Customer.DaytimePhone,
		Customer.MobilePhone,
		#Shops.Shops,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), #MaxOffer.MaxApproved)), 1), 2) MaxApproved,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), #SumOfLoans.SumOfLoans)), 1), 2) SumOfLoans,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), T.AnualTurnover)), 1), 2) AnualTurnover,
		T.ExpirianRating,
		T.MedalType,
		CASE T.MedalType
			WHEN 'Silver'   THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.06, 0))), 1), 2)
			WHEN 'Gold'     THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.08, 0))), 1), 2)
			WHEN 'Platinum' THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.10, 0))), 1), 2)
			WHEN 'Diamond'  THEN PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), ROUND(T.AnualTurnover * 0.12, 0))), 1), 2)
		END PhoneOffer
	FROM
		Customer
		LEFT JOIN #Shops ON Customer.Id = #Shops.CustomerId
		LEFT JOIN #MaxOffer ON Customer.Id = #MaxOffer.CustomerId
		LEFT JOIN #SumOfLoans ON Customer.Id = #SumOfLoans.CustomerId
		LEFT JOIN #AnualTurnover T ON Customer.Id = T.CustomerId
	WHERE
		Customer.IsTest = 0
		AND
		Name NOT LIKE '%ezbob%'
		AND
		Name NOT LIKE '%liatvanir%'
		AND
		CONVERT(DATE, @DateStart) <= Customer.GreetingMailSentDate AND Customer.GreetingMailSentDate < CONVERT(DATE, @DateEnd)
	GROUP BY
		Customer.Id,
		Customer.Name,
		Customer.FirstName,
		Customer.Surname,
		Customer.DaytimePhone,
		Customer.MobilePhone,
		Customer.GreetingMailSentDate,
		#Shops.Shops,
		#MaxOffer.MaxApproved,
		#SumOfLoans.SumOfLoans,
		T.ExpirianRating,
		T.AnualTurnover,
		T.MedalType

	DROP TABLE #Shops
	DROP TABLE #MaxOffer
	DROP TABLE #SumOfLoans
	DROP TABLE #tmp1
	DROP TABLE #AnualTurnover
END
GO
