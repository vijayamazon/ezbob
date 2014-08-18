IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptApprovedDidntTake]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptApprovedDidntTake]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptApprovedDidntTake] 
	(@DateStart DATETIME,
				@DateEnd DATETIME)
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
 
if OBJECT_ID('tempdb..#temp4') is not NULL
BEGIN
                DROP TABLE #temp4
END

if OBJECT_ID('tempdb..#temp5') is not NULL
BEGIN
                DROP TABLE #temp5
END
--------------------------------------------------

--------- GET LATEST DATE OF APPLICATION ---------
SELECT R.IdCustomer,max(R.CreationDate) AS MaxAppDate
INTO #temp1
FROM CashRequests R
GROUP BY R.IdCustomer


------------ GET APPROVED CLIENT LIST  -----------
SELECT T.IdCustomer,T.MaxAppDate
INTO #temp2
FROM #temp1 T
JOIN CashRequests R ON R.IdCustomer = T.IdCustomer
WHERE R.UnderwriterDecision = 'approved'
	  AND T.MaxAppDate = R.CreationDate



SELECT  T.IdCustomer,
                                C.Name,
                    convert(DATE,C.GreetingMailSentDate) AS SignUpDate,
                                C.Fullname,
                                C.DaytimePhone,
                                C.MobilePhone,
                                C.ReferenceSource,
                                T.MaxAppDate AS ApprovedDate,
                                R.ExpirianRating,
                                R.UnderwriterDecision,
                                R.UnderwriterDecisionDate,
                                R.ManagerApprovedSum,
                                R.InterestRate,
                                R.RepaymentPeriod,
                                CASE
                                WHEN R.LoanTypeId = 2 THEN 'HalfWay Loan'
                                WHEN R.LoanTypeId = 1 THEN 'Standard Loan'
                                END AS LoansType,
                                R.Id AS CashReqId,
                                C.BrokerID
               
INTO #temp3
FROM #temp2 T
JOIN Customer C ON C.Id = T.IdCustomer
JOIN CashRequests R ON R.IdCustomer = T.IdCustomer
WHERE T.IdCustomer NOT IN
                                                                                                  ( SELECT C.Id
                                                                                                                FROM Customer C
                                                                                                                WHERE Name LIKE '%ezbob%'
                                                                                                                OR Name LIKE '%liatvanir%'
                                                                                                                OR Name LIKE '%q@q%'
                                                                                                                OR Name LIKE '%1@1%'
                                                                                                                OR CollectionStatus != 0
                                                                                                                OR C.IsTest=1) 
                  AND T.MaxAppDate = R.CreationDate
 
 
SELECT L.Id,L.[Date],L.LoanAmount,L.RequestCashId
INTO #temp4
FROM Loan L
 
---------- COUNT OF PREVIOUS LOANS FOR CUSTOMER ----------
SELECT L.CustomerId,count(L.Id) AS LoanIssued
INTO #temp5
FROM Loan L
GROUP BY L.CustomerId
 
----------------------------------------------------------
SELECT  T1.IdCustomer,
                                T1.Fullname,
                                T1.Name AS Email,
                                T1.ReferenceSource,
                                T1.ExpirianRating,
                                T1.UnderwriterDecisionDate,
                                T1.ManagerApprovedSum,
                                T1.InterestRate,
                                T1.RepaymentPeriod,
                                T1.LoansType,
                                CASE
                                WHEN T3.LoanIssued>0 THEN 'Yes'
                                ELSE 'No'
                                END AS HadLoan,
                                T1.BrokerID
 
FROM #temp3 T1
LEFT JOIN #temp4 T2 ON T1.CashReqId = T2.RequestCashId
LEFT JOIN #temp5 T3 ON T3.CustomerId = T1.IdCustomer
WHERE T2.LoanAmount IS NULL
                  AND T1.UnderwriterDecisionDate BETWEEN @DateStart AND @DateEnd
ORDER BY 7
 
SET NOCOUNT OFF
END
 
GO
