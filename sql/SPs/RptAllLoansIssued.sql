IF OBJECT_ID('RptAllLoansIssued') IS NULL
	EXECUTE('CREATE PROCEDURE RptAllLoansIssued AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAllLoansIssued
AS
BEGIN
	SET NOCOUNT ON;

	------------- TEMP TABLES CREATION ---------------

	IF OBJECT_ID('tempdb..#temp1') IS NOT NULL
		DROP TABLE #temp1

	IF OBJECT_ID('tempdb..#temp2') IS NOT NULL
		DROP TABLE #temp2

	IF OBJECT_ID('tempdb..#temp3') IS NOT NULL
		DROP TABLE #temp3

	-------------- GET LOAN ID, AMOUNT & OTHER BASIC INFO -------------

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
		RANK() OVER (PARTITION BY L.CustomerId ORDER BY L.[Date],L.Id DESC) AS LoanNumber,
		R.IsLoanTypeSelectionAllowed AS CustSelection,
		CASE
			WHEN R.DiscountPlanId = 1 THEN '0'
			WHEN R.DiscountPlanId = 3 THEN 'New13'
			WHEN R.DiscountPlanId = 4 THEN 'Old13'
		END AS HasDiscountPlan,
		L.LoanSourceID,
		L.Principal AS OutstandingPrincipal
	INTO
		#temp1
	FROM
		Loan L
		JOIN CashRequests R ON R.Id = L.RequestCashId
	WHERE
		L.CustomerId NOT IN (
			SELECT C.Id
			FROM Customer C
			WHERE Name LIKE '%ezbob%'
			OR Name LIKE '%liatvanir%'
			OR Name LIKE '%q@q%'
			OR Name LIKE '%1@1%'
			OR C.IsTest = 1
		)
		AND
		L.[Date] > '2012-08-31 23:59'
	ORDER BY
		1,
		3

	------------- LIST OF ALL LOANS BY CUSTOMER AND SOURCE REF AND LOAN RANK (NEW/OLD) -----------------

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
			WHEN T.LoanNumber = 1 THEN R.Amount
		END AS CustomerRequestedAmount,
		C.ReferenceSource,
		CASE
			WHEN C.BrokerID IS NOT NULL THEN 'Broker'
			WHEN (S.RSource = 'Money_co_uk' OR S.RSource = 'moneycouk') THEN 'Money.co.uk'
			ELSE S.RSource
		END AS SourceRefGroup,
		S.RMedium,
		C.IsOffline,
		CASE
			WHEN T.LoanSourceID = 1 THEN 'Standard'
			WHEN T.LoanSourceID = 2 THEN 'EU'
			WHEN T.LoanSourceID = 3 THEN 'COSME'
		END AS Loan_Type,
		CASE
			WHEN C.BrokerID IS NOT NULL THEN 'BrokerClient'
			ELSE 'NonBroker'
		END AS BrokerOrNot,
		CASE
			WHEN T.LoanDate BETWEEN '2012-07-01' AND '2013-01-01' THEN 'Q4-2012'
			WHEN T.LoanDate BETWEEN '2013-01-01' AND '2013-04-01' THEN 'Q1-2013'
			WHEN T.LoanDate BETWEEN '2013-04-01' AND '2013-07-01' THEN 'Q2-2013'
			WHEN T.LoanDate BETWEEN '2013-07-01' AND '2013-10-01' THEN 'Q3-2013'
			WHEN T.LoanDate BETWEEN '2013-10-01' AND '2014-01-01' THEN 'Q4-2013'
			WHEN T.LoanDate BETWEEN '2014-01-01' AND '2014-04-01' THEN 'Q1-2014'
			WHEN T.LoanDate BETWEEN '2014-04-01' AND '2014-07-01' THEN 'Q2-2014'
			WHEN T.LoanDate BETWEEN '2014-07-01' AND '2014-10-01' THEN 'Q3-2014'
			WHEN T.LoanDate BETWEEN '2014-10-01' AND '2015-01-01' THEN 'Q4-2014'
			WHEN T.LoanDate BETWEEN '2015-01-01' AND '2015-04-01' THEN 'Q1-2015'
			WHEN T.LoanDate BETWEEN '2015-04-01' AND '2015-07-01' THEN 'Q2-2015'
			WHEN T.LoanDate BETWEEN '2015-07-01' AND '2015-10-01' THEN 'Q3-2015'
			WHEN T.LoanDate BETWEEN '2015-10-01' AND '2016-01-01' THEN 'Q4-2015'
			ELSE 'No Q'
		END AS Quarter,
		CASE
			WHEN C.AlibabaId IS NULL THEN 'NotAlibaba'
			ELSE 'Alibaba'
		END AS AlibabaOrNot
	INTO
		#temp2       
	FROM
		#temp1 T
		JOIN Customer C ON T.CustomerId = C.Id
		LEFT JOIN CampaignSourceRef S ON S.CustomerId = T.CustomerId
		LEFT JOIN CustomerRequestedLoan R ON R.CustomerId = T.CustomerId
	ORDER BY
		1,
		10

	----------------- GET 1ST LOAN DATE -----------------

	SELECT
		L.CustomerId,
		MIN(L.[Date]) AS FirstLoanDate
	INTO
		#temp3
	FROM
		Loan L
	WHERE
		L.CustomerId NOT IN (
			SELECT C.Id
			FROM Customer C
			WHERE Name LIKE '%ezbob%'
			OR Name LIKE '%liatvanir%'
			OR Name LIKE '%q@q%'
			OR Name LIKE '%1@1%'
			OR C.IsTest = 1
		)
	GROUP BY
		L.CustomerId

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
			WHEN (T.LoanNumber = 2 AND (datediff(day,T3.FirstLoanDate,T.LoanDate) < 2)) THEN 'New'
			ELSE 'Old'
		END AS NewOldLoan,
		T.AlibabaOrNot
	FROM
		#temp2 T
		JOIN #temp3 T3 ON T3.CustomerId = T.CustomerId
END
GO
