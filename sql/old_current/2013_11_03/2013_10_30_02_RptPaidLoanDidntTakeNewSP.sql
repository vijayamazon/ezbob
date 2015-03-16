IF OBJECT_ID('RptPaidLoanDidntTakeNew') IS NULL
	EXECUTE('CREATE PROCEDURE RptPaidLoanDidntTakeNew AS SELECT 1')
GO

ALTER PROCEDURE RptPaidLoanDidntTakeNew

				 @DateStart DATETIME,
				 @DateEnd DATETIME

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


-------- LATEST CASHREQUEST DATE -------
SELECT R.IdCustomer,max(R.CreationDate) AS MaxCreationDate
INTO #temp1
FROM CashRequests R
WHERE 	R.IdCustomer NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1)
GROUP BY R.IdCustomer


-------- GET LATEST EXPERIAN SCORE --------

SELECT T.IdCustomer,R.ExpirianRating,T.MaxCreationDate,R.UnderwriterDecision,R.UnderwriterComment
INTO #temp2
FROM #temp1 T
JOIN CashRequests R ON R.IdCustomer = T.IdCustomer
WHERE T.MaxCreationDate = R.CreationDate


-------- LIST OF CUSTOMERS WHO PAID OFF LOANS AND DIDNT TAKE NEW ONE -----
SELECT L.CustomerId,
	   C.Name,
	   C.Fullname,
	   count(L.[Date]) AS NumOfLoansTook,
	   T.ExpirianRating,
	   max(L.DateClosed) AS DatePaidLastLoan,
	   C.DaytimePhone,
	   C.MobilePhone,
	   C.NonLimitedBusinessPhone,
	   C.LimitedBusinessPhone,
	   max(R.ManagerApprovedSum) AS MaxApprovedSum,
	   T.MaxCreationDate AS LastDecisionDate,
	   T.UnderwriterDecision AS LastUnderwriterDecision,
	   T.UnderwriterComment
FROM Loan L
JOIN #temp2 T ON T.IdCustomer = L.CustomerId
JOIN Customer C ON C.Id = L.CustomerId
JOIN CashRequests R ON R.Id = L.RequestCashId
WHERE 	L.CustomerId NOT IN 
						  ( SELECT C.Id
							FROM Customer C 
							WHERE Name LIKE '%ezbob%'
							OR Name LIKE '%liatvanir%'
							OR Name LIKE '%q@q%'
							OR Name LIKE '%1@1%'
							OR C.IsTest=1
							)
		AND C.CollectionStatus = 0
GROUP BY L.CustomerId,
		 C.Name,
		 C.Fullname,
		 C.DaytimePhone,
		 C.MobilePhone,
		 C.NonLimitedBusinessPhone,
		 C.LimitedBusinessPhone,
		 T.ExpirianRating,
		 T.UnderwriterDecision,
		 T.UnderwriterComment,
		 T.MaxCreationDate
HAVING count(L.[Date]) = count(L.DateClosed)
	   AND max(L.DateClosed) BETWEEN @DateStart AND @DateEnd	
ORDER BY 6

DROP TABLE #temp1
DROP TABLE #temp2

END 

GO 
