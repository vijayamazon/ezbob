IF OBJECT_ID('EzTvGetStats') IS NULL
	EXECUTE('CREATE PROCEDURE EzTvGetStats AS SELECT 1')
GO

ALTER PROCEDURE EzTvGetStats
@MonthAgo DATETIME
AS
BEGIN

DECLARE @DefaultCustomerCount INT = (SELECT count(*) FROM Customer c JOIN CustomerStatuses s ON s.Id = c.CollectionStatus WHERE s.IsDefault=1 AND c.IsTest=0)
DECLARE @CustomerCount INT = (SELECT count(*) FROM Customer c JOIN Loan l ON c.Id=l.CustomerId WHERE c.IsTest=0)

SELECT 'DefaultRate' AS 'Key', @DefaultCustomerCount / CAST(@CustomerCount AS DECIMAL(18,6)) AS Value 

UNION

SELECT 'AvgDailyLoans' AS 'Key', sum(Amount) / CAST(count(*) AS DECIMAL(18,6)) AS Value  FROM vw_LoansAmountByDay

UNION

SELECT 'TotalLoans' AS 'Key', CAST(sum(Amount) AS DECIMAL(18,6)) AS Value  FROM vw_LoansAmountByDay

UNION

SELECT 'AvgNewLoan' AS 'Key', sum(LoanAmount) / CAST(count(*) AS DECIMAL(18,6)) AS Value FROM Loan WHERE [Date]>=@MonthAgo AND CustomerId IN (
	SELECT c.Id
	FROM Loan l JOIN Customer c ON l.CustomerId=c.Id 
	JOIN CustomerStatuses s ON s.Id = c.CollectionStatus
	WHERE c.IsTest=0
	AND s.IsDefault=0
	GROUP BY c.Id
	HAVING count(l.Id)=1
)	
UNION

SELECT 'AvgInterest' AS 'Key', sum(l.InterestRate) / CAST(count(*) AS DECIMAL(18,6)) AS Value FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE c.IsTest=0

UNION

SELECT 'AvgLoanSize' AS 'Key', sum(l.LoanAmount) / CAST(count(*) AS DECIMAL(18,6)) AS Value FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE c.IsTest=0

UNION

SELECT 'BookSize' AS 'Key', CAST(sum(l.Balance) AS DECIMAL(18,6)) AS Value 
FROM Loan l JOIN Customer c ON l.CustomerId=c.Id 
JOIN CustomerStatuses s ON s.Id = c.CollectionStatus
WHERE s.IsDefault=0
AND c.IsTest=0

UNION

SELECT 'OpenBugs' AS 'Key', CAST(count(*) AS DECIMAL(18,6)) AS Value  FROM Bug WHERE State='Opened'

UNION

SELECT 'TodayLoans' AS 'Key', CAST(COALESCE(sum(l.LoanAmount),0) AS DECIMAL(18,6)) AS Value  FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE datediff(day, [Date], getdate()) = 0 AND c.IsTest=0


END 

GO
