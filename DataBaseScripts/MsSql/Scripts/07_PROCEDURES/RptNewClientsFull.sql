IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewClientsFull]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewClientsFull]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptNewClientsFull
@DateStart    DATETIME,
@DateEnd      DATETIME
AS
BEGIN
	IF OBJECT_ID('tempdb..#tmp1') is not NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#tmp2') is not NULL
		DROP TABLE #tmp2

	IF OBJECT_ID('tempdb..#tmpOffer') is not NULL
		DROP TABLE #tmpOffer

	SELECT
		CustomerId,
		count(MarketPlaceId) Shops
	INTO
		#tmp2
	FROM
		MP_CustomerMarketPlace
	GROUP BY
		CustomerId

	SELECT
		IdCustomer CustomerId,
		MAX(ManagerApprovedSum) MaxApproved
	INTO
		#tmpOffer
	FROM
		CashRequests
	GROUP BY
		IdCustomer

	SELECT
		Name,
		Customer.FirstName,
		Customer.Surname,
		Customer.DaytimePhone,
		Customer.MobilePhone,
		DATEPART(dw, GreetingMailSentDate) DayOfWeek,
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
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), #tmpOffer.MaxApproved)), 1), 2) MaxApproved,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), Loan.LoanAmount)), 1), 2) LoanAmount,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), Customer.CreditSum)), 1), 2) CreditLeft,
		Loan.[Date],
		DATEPART(dw, Loan.[Date]) LoanDayOfWeek
	INTO
		#tmp1
	FROM
		Customer
		LEFT JOIN #tmp2 ON Customer.Id = #tmp2.CustomerId
		LEFT JOIN Loan ON loan.CustomerId = Customer.Id
		LEFT JOIN #tmpOffer ON #tmpOffer.CustomerId = Customer.Id
	WHERE
		CONVERT(DATE, @DateStart) <= GreetingMailSentDate AND GreetingMailSentDate < CONVERT(DATE, @DateEnd)
		AND
		IsTest = 0

	SELECT
		*
	FROM
		#tmp1
	WHERE
		Name NOT LIKE '%ezbob%'
		AND
		Name NOT LIKE '%q.q%'
		AND
		Name NOT LIKE '%liatvanir%'

	DROP TABLE #tmp1
	DROP TABLE #tmp2
	DROP TABLE #tmpOffer
END
GO
