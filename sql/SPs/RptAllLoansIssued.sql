IF OBJECT_ID('RptAllLoansIssued') IS NULL
	EXECUTE('CREATE PROCEDURE RptAllLoansIssued AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAllLoansIssued
 
AS
BEGIN
 
SET NOCOUNT ON
 
------------- TEMP TABLES CREATION ---------------
if OBJECT_ID('tempdb..#temp1') is not NULL
BEGIN
                DROP TABLE #temp1
END
 
if OBJECT_ID('tempdb..#temp2') is not NULL
BEGIN
                DROP TABLE #temp2
END
 
if OBJECT_ID('tempdb..#temp3') is not NULL
BEGIN
                DROP TABLE #temp3
END
 
-------------- GET LOAN ID, AMOUNT & OTHER BASIC INFO -------------
SELECT L.CustomerId,
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
INTO #temp1
FROM Loan L
JOIN CashRequests R ON R.Id = L.RequestCashId
WHERE L.CustomerId NOT IN
                                                                                                  ( SELECT C.Id
                                                                                                                FROM Customer C
                                                                                                                WHERE Name LIKE '%ezbob%'
                                                                                                                OR Name LIKE '%liatvanir%'
                                                                                                                OR Name LIKE '%q@q%'
                                                                                                                OR Name LIKE '%1@1%'
                                                                                                                OR C.IsTest=1)
                  AND L.[Date] > '2012-08-31 23:59'
ORDER BY 1,3
 
 
------------- LIST OF ALL LOANS BY CUSTOMER AND SOURCE REF AND LOAN RANK (NEW/OLD) -----------------
SELECT T.CustomerId,
                   T.LoanID,
                   T.LoanDate,
                   DATEADD(MONTH,DATEDIFF(MONTH, 0,T.LoanDate), 0) AS IssueMonth,
                   T.LoanAmount,
                   T.InterestRate,
                   T.RepaymentPeriod,
                   T.OutstandingPrincipal,
                   T.SetupFee,
                                                                                                                WHERE Name LIKE '%ezbob%'
                                                                                                                OR Name LIKE '%liatvanir%'
                                                                                                                OR Name LIKE '%q@q%'
                                                                                                                OR Name LIKE '%1@1%'
                                                                                                                OR C.IsTest=1)
GROUP BY L.CustomerId
 
SELECT T.CustomerId,
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
                   T.IsOffline,
                   T.Loan_Type,
                   T.BrokerOrNot,
                   T.Quarter,
                   CASE
                   WHEN T.LoanNumber = 1 THEN 'New'
                   WHEN (T.LoanNumber = 2 AND (datediff(day,T3.FirstLoanDate,T.LoanDate) < 2)) THEN 'New'
                   ELSE 'Old'
                   END AS NewOldLoan
                  
FROM #temp2 T
JOIN #temp3 T3 ON T3.CustomerId = T.CustomerId
 
END;
GO