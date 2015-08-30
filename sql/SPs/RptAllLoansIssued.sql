IF OBJECT_ID('RptAllLoansIssued') IS NULL
	EXECUTE('CREATE PROCEDURE RptAllLoansIssued AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAllLoansIssued
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------
	--
	-- GET LOAN ID, AMOUNT & OTHER BASIC INFO
	--
	------------------------------------------------------------------------------

	SELECT
		L.CustomerId,
		L.Id AS LoanID,
		L.[Date] AS LoanDate,
		L.LoanAmount,
		L.InterestRate,
		L.SetupFee,
		R.RepaymentPeriod,
		R.ManualSetupFeeAmount,
		R.ManualSetupFeePercent,
		RANK() OVER (PARTITION BY L.CustomerId ORDER BY L.[Date], L.Id DESC) AS LoanNumber,
		R.IsLoanTypeSelectionAllowed AS CustSelection,
		CASE
			WHEN R.DiscountPlanId = 1 THEN '0'
			WHEN R.DiscountPlanId = 3 THEN 'New13'
			WHEN R.DiscountPlanId = 4 THEN 'Old13'
		END AS HasDiscountPlan,
		L.LoanSourceID,
		L.Principal AS OutstandingPrincipal
	INTO
		#loans
	FROM
		Loan L
		INNER JOIN Customer C ON L.CustomerId = C.Id AND C.IsTest = 0
		INNER JOIN CashRequests R ON R.Id = L.RequestCashId
	WHERE
		L.[Date] >= 'September 1 2012'
	ORDER BY
		1,
		3

		
	------------------------------------------------------------------------------
	--
	-- LIST OF ALL LOANS BY CUSTOMER AND SOURCE REF AND LOAN RANK (NEW/OLD)
	--
	------------------------------------------------------------------------------
	;WITH
	crl_id AS (
		SELECT
			CustomerId,
			MAX(Id) AS Id
		FROM
			CustomerRequestedLoan
		GROUP BY
			CustomerId
	),
	crl AS (
		SELECT
			c.CustomerId,
			c.Amount AS Amount
		FROM
			crl_id d
			INNER JOIN CustomerRequestedLoan c ON d.Id = c.Id
	)
	SELECT
		T.CustomerId,
		T.LoanID,
		T.LoanDate,
		DATEADD(MONTH, DATEDIFF(MONTH, 0, T.LoanDate), 0) AS IssueMonth,
		T.LoanAmount,
		T.InterestRate,
		T.RepaymentPeriod,
		T.OutstandingPrincipal,
		T.SetupFee,
		T.LoanNumber,
		CASE
			WHEN T.LoanNumber = 1 THEN crl.Amount
		END AS CustomerRequestedAmount,
		C.ReferenceSource,
		CASE
			WHEN C.BrokerID IS NOT NULL THEN 'Broker'
			WHEN (S.RSource = 'Money_co_uk' OR S.RSource = 'moneycouk') THEN 'Money.co.uk'
			ELSE S.RSource
		END AS SourceRefGroup,
		S.RMedium,
		C.IsOffline,
		ls.LoanSourceName AS Loan_Type,
		CASE
			WHEN C.BrokerID IS NOT NULL THEN 'BrokerClient'
			ELSE 'NonBroker'
		END AS BrokerOrNot,
		'Q' + CONVERT(VARCHAR(1), DATEPART(q, T.LoanDate)) + '-' + CONVERT(VARCHAR(4), DATEPART(year, T.LoanDate)) AS Quarter,
		CASE
			WHEN C.AlibabaId IS NULL THEN 'NotAlibaba'
			ELSE 'Alibaba'
		END AS AlibabaOrNot
	INTO
		#loan_customer       
	FROM
		#loans T
		INNER JOIN Customer C ON T.CustomerId = C.Id
		INNER JOIN LoanSource ls ON T.LoanSourceID = ls.LoanSourceID
		LEFT JOIN CampaignSourceRef S ON S.CustomerId = T.CustomerId
		LEFT JOIN crl on crl.CustomerId = C.Id
	ORDER BY
		1,
		10

	------------------------------------------------------------------------------
	--
	-- GET 1ST LOAN DATE
	--
	------------------------------------------------------------------------------

	SELECT
		L.CustomerId,
		MIN(L.[Date]) AS FirstLoanDate
	INTO
		#first_loan
	FROM
		Loan L
		INNER JOIN Customer C ON L.CustomerId = C.Id AND C.IsTest = 0
	GROUP BY
		L.CustomerId

	------------------------------------------------------------------------------
	--
	-- Output
	--
	------------------------------------------------------------------------------

	SELECT
		T.CustomerId,
		T.LoanID,
		T.LoanDate,
		T.IssueMonth,
		T.LoanAmount,
		T.InterestRate,
		T.RepaymentPeriod,
		T.OutstandingPrincipal,
		T.SetupFee,
		T.LoanNumber,
		T.CustomerRequestedAmount,
		T.ReferenceSource,
		T.SourceRefGroup,
		T.RMedium AS SourceRefMedium,
		T.IsOffline,
		T.Loan_Type,
		T.BrokerOrNot,
		T.Quarter,
		CASE
			WHEN T.LoanNumber = 1 THEN 'New'
			WHEN (T.LoanNumber = 2 AND (DATEDIFF(day, T3.FirstLoanDate, T.LoanDate) < 2)) THEN 'New'
			ELSE 'Old'
		END AS NewOldLoan,
		T.AlibabaOrNot
	FROM
		#loan_customer T
		INNER JOIN #first_loan T3 ON T3.CustomerId = T.CustomerId

	------------------------------------------------------------------------------

	DROP TABLE #first_loan
	DROP TABLE #loan_customer
	DROP TABLE #loans
END
GO
