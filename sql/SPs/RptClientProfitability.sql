IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptClientProfitability]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptClientProfitability]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptClientProfitability]
AS
BEGIN
	SET NOCOUNT ON

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

if OBJECT_ID('tempdb..#tmp1') is not NULL
BEGIN
	DROP TABLE #tmp1
END

if OBJECT_ID('tempdb..#tmp2') is not NULL
BEGIN
	DROP TABLE #tmp2
END

---------------------------------------------------------------
------------------- RECENT CREDIT SCORE -----------------------
SELECT R.IdCustomer,max(R.CreationDate) LastDate
INTO #temp1
FROM CashRequests R
GROUP BY R.IdCustomer


SELECT T.IdCustomer,R.ExpirianRating
INTO #temp2
FROM #temp1 T
JOIN CashRequests R ON R.IdCustomer = T.IdCustomer
WHERE T.LastDate = R.CreationDate
	  AND T.IdCustomer NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1) 	
ORDER BY 1

------------------- ALL PAYMENTS TRANSACTIONS -----------------------
SELECT T.LoanId,
	   L.CustomerId,
	   sum(T.LoanRepayment) AS PrincipalPaid,
	   sum(T.Interest) AS InterestPaid,
	   sum(T.Fees) AS FeesPaid,
	   sum(T.Amount) AS TotalPaid
INTO #tmp1
FROM LoanTransaction T
JOIN Loan L ON L.Id = T.LoanId
WHERE T.LoanId NOT IN (SELECT L.Id
					   FROM Loan L
					   JOIN Customer C ON C.Id = L.CustomerId
					   WHERE C.Name LIKE '%ezbob%'
							 OR C.Name LIKE '%liatvanir%'
							 OR C.Name LIKE '%q@q%'
							 OR C.Name LIKE '%1@1%'
							 OR C.IsTest=1) 
	  AND T.Status = 'Done'
	  AND T.Type = 'PaypointTransaction'	
GROUP BY T.LoanId,L.CustomerId
ORDER BY 2,1


SELECT T.CustomerId,
	   sum(T.PrincipalPaid) AS PrincipalPaid,
	   sum(T.InterestPaid) AS InterestPaid,
	   sum(T.FeesPaid) AS FeesPaid,
	   sum(T.TotalPaid) AS TotalPaid
INTO #tmp2
FROM #tmp1 T
GROUP BY T.CustomerId

------------------ FINAL TABLE ------------------------
SELECT L.CustomerId,
	   C.Fullname,
	   CASE 
	   WHEN C.CollectionStatus = 0 THEN 'Enabled'
	   WHEN C.CollectionStatus = 1 THEN 'Disabled'
	   WHEN C.CollectionStatus = 2 THEN 'Fraud'
	   WHEN C.CollectionStatus = 3 THEN 'Legal'
	   WHEN C.CollectionStatus = 4 THEN 'Default'
	   WHEN C.CollectionStatus = 5 THEN 'Fraud Suspect'
	   WHEN C.CollectionStatus = 6 THEN 'Risky'
	   WHEN C.CollectionStatus = 7 THEN 'Bad'
	   WHEN C.CollectionStatus = 8 THEN 'WriteOff'
	   WHEN C.CollectionStatus = 9 THEN 'Debt Management'
	   END AS CustomerStatus,
	   T2.ExpirianRating AS CreditScore,
	   count(L.Id) AS NumOfLoansIssued,
	   round(sum(L.LoanAmount),0) AS TotalAmountIssued,
	   round(sum(L.Principal),0) AS OutstandingPrincipal,
	   round(T.PrincipalPaid,0) AS PrincipalPaid,
	   round(T.InterestPaid,0) AS InterestPaid,
	   round(T.FeesPaid,0) AS FeesPaid,
	   round(T.TotalPaid,0) AS TotalPaid,
	   round(T.InterestPaid + T.FeesPaid,0) AS TotalIncome
INTO #temp3	   
FROM Loan L
JOIN Customer C ON C.Id = L.CustomerId
JOIN #temp2 T2 ON T2.IdCustomer = L.CustomerId
LEFT JOIN #tmp2 T ON T.CustomerId = L.CustomerId
WHERE L.CustomerId NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1) 
GROUP BY  L.CustomerId,
		  C.Fullname,
		  C.CollectionStatus,
		  T2.ExpirianRating,
		  T.PrincipalPaid,
	   	  T.InterestPaid,
	      T.FeesPaid,
	      T.TotalPaid
ORDER BY 1
---------------------------------------------------------------

SELECT T.CustomerId,
	   T.Fullname,
	   CASE 
			WHEN C.BrokerID IS NOT NULL             THEN 'Broker'
			WHEN C.ReferenceSource LIKE 'eBay%'		THEN 'eBay'
			WHEN C.ReferenceSource LIKE 'Amazon%'	THEN 'Amazon'
			WHEN C.ReferenceSource LIKE 'Ekm%'		THEN 'EKM'
			WHEN C.ReferenceSource LIKE 'adwords%'	THEN 'Google'
			WHEN C.ReferenceSource LIKE 'Google%'	THEN 'Google'
			WHEN C.ReferenceSource LIKE 'adroll%'	THEN 'Adroll'
			WHEN C.ReferenceSource LIKE 'Bing%'		THEN 'Bing'
			WHEN C.ReferenceSource LIKE 'tam%'		THEN 'Tamebay'
			WHEN C.ReferenceSource LIKE 'ChannelG%'	THEN 'ChannelGrabber'
			WHEN C.ReferenceSource IS NULL 			THEN 'No SourceRef'
			ELSE 'Other'
	   END AS SourceRefGroup,
	   T.CustomerStatus,
	   T.CreditScore,
	   T.NumOfLoansIssued,
	   T.TotalAmountIssued,
	   T.OutstandingPrincipal,
	   T.PrincipalPaid,
	   T.InterestPaid,
	   T.FeesPaid,
	   T.TotalPaid,
	   T.TotalIncome,
	   CASE 
	   WHEN T.CustomerStatus != 'Enabled' THEN (T.TotalIncome - T.OutstandingPrincipal)
	   ELSE (T.TotalIncome)
	   END AS IncomeAfterDef,
	   round(T.TotalIncome/T.TotalAmountIssued,3) AS IncomeDivIssued	   	
FROM #temp3 T
JOIN Customer C ON C.Id = T.CustomerId


----------------DROP TEMP TABLES ------------------------------
DROP TABLE #temp1
DROP TABLE #temp2
DROP TABLE #temp3
DROP TABLE #tmp1
DROP TABLE #tmp2
END
GO
