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
                   T.ManualSetupFeeAmount,
                   (T.ManualSetupFeePercent*T.LoanAmount) AS SetupFee,
                   T.LoanNumber,
                   CASE
                   WHEN T.LoanNumber = 1 THEN R.Amount
                   END AS CustomerRequestedAmount,
                   C.ReferenceSource,
                   CASE
 
                                   WHEN C.ReferenceSource LIKE 'eBay%'                              THEN 'eBay'
                                   WHEN C.ReferenceSource LIKE 'Amazon%'       THEN 'Amazon'
                                   WHEN C.ReferenceSource LIKE 'Ekm%'                               THEN 'EKM'
                                   WHEN C.ReferenceSource LIKE 'adwords%'      THEN 'Google'
                                   WHEN C.ReferenceSource LIKE 'Google%'         THEN 'Google'
                                   WHEN C.ReferenceSource LIKE 'bros%'                               THEN 'Google'
                                   WHEN C.ReferenceSource LIKE '%mobile%'      THEN 'Google'
                                   WHEN C.ReferenceSource LIKE 'comp-%'                           THEN 'Google'
                                   WHEN C.ReferenceSource LIKE 'ezbob-real%' THEN 'Google'
                                   WHEN C.ReferenceSource LIKE 'adroll%'            THEN 'Adroll'
                                   WHEN C.ReferenceSource LIKE 'Bing%'                               THEN 'Bing'
                                   WHEN C.ReferenceSource LIKE 'tam%'                                THEN 'Tamebay'
                                   WHEN C.ReferenceSource LIKE 'ChannelG%'    THEN 'ChannelGrabber'
                                   WHEN C.ReferenceSource LIKE 'm.co.uk'           THEN 'Money.co.uk'
                                   WHEN C.ReferenceSource LIKE 'Money.co.uk' THEN 'Money.co.uk'
                                   WHEN C.ReferenceSource LIKE 'money_co_uk' THEN 'Money.co.uk'
                                   WHEN C.ReferenceSource LIKE 'm-co-uk%'       THEN 'Money.co.uk'
                                   WHEN C.ReferenceSource LIKE 'm_co_uk'        THEN 'Money.co.uk'
                                   WHEN C.ReferenceSource LIKE 'Moneycouk%'               THEN 'Money.co.uk'
                                   WHEN C.ReferenceSource LIKE 'msm%'                              THEN 'MoneySuperMarket'
                                   WHEN C.ReferenceSource LIKE 'Broker'                              THEN 'Broker'
                                   WHEN C.ReferenceSource IS NULL                                        THEN 'No SourceRef'
                                   ELSE 'Other'
                   END AS SourceRefGroup,
                   C.IsOffline,
                   CASE
                   WHEN T.LoanSourceID = 1 THEN 'Standard'
                   ELSE 'EU'
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
                                                ELSE 'No Q'
                                END AS Quarter
 
                  
INTO #temp2       
FROM #temp1 T
JOIN Customer C ON T.CustomerId = C.Id
LEFT JOIN CustomerRequestedLoan R ON R.CustomerId = T.CustomerId
ORDER BY 1,10
 
----------------- GET 1ST LOAN DATE -----------------
 
SELECT L.CustomerId,min(L.[Date]) AS FirstLoanDate
INTO #temp3
FROM Loan L
WHERE L.CustomerId NOT IN
                                                                                                  ( SELECT C.Id
                                                                                                                FROM Customer C
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
                   T.ManualSetupFeeAmount,
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

