
IF OBJECT_ID('EzTvGetStats') IS NULL
	EXECUTE('CREATE PROCEDURE EzTvGetStats AS SELECT 1')
GO

ALTER PROCEDURE EzTvGetStats
@Now DATETIME,
@FirstOfMonth DATETIME,
@MonthAgo DATETIME
AS
BEGIN

DECLARE @DefaultCustomerCount INT = (SELECT count(*) FROM Customer c JOIN CustomerStatuses s ON s.Id = c.CollectionStatus WHERE s.IsDefault=1 AND c.IsTest=0)
DECLARE @CustomerCount INT = (SELECT count(*) FROM Customer c JOIN Loan l ON c.Id=l.CustomerId WHERE c.IsTest=0)

DECLARE @TotalGivenLoanValueClose DECIMAL(18,6)
DECLARE @TotalRepaidPrincipalClose DECIMAL(18,6)

DECLARE @FirstLoanDate DATETIME = (SELECT min([Date]) FROM Loan)
DECLARE @DaysFromFirstLoan INT = (select DATEDIFF(d, @FirstLoanDate, @Now))

SELECT
	@TotalGivenLoanValueClose = ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
FROM
	LoanTransaction t
	INNER JOIN Loan l ON t.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
WHERE
	t.Type = 'PacnetTransaction' AND t.Status = 'Done'
	AND
	t.PostDate < @Now

------------------------------------------------------------------------------

SELECT
	@TotalRepaidPrincipalClose = ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
FROM
	LoanTransaction t
	INNER JOIN Loan l ON t.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
WHERE
	t.Type = 'PaypointTransaction' AND t.Status = 'Done'
	AND
	t.PostDate < @Now
		
--------------------------------------------------------------------------------
--------------------------------------------------------------------------------
--------------------------------------------------------------------------------

SELECT 'G_DefaultRate' AS 'Key', @DefaultCustomerCount / CAST(@CustomerCount AS DECIMAL(18,6)) AS Value 

UNION

SELECT 'G_AvgDailyLoans' AS 'Key', sum(Amount) / CAST(@DaysFromFirstLoan AS DECIMAL(18,6)) AS Value  FROM vw_LoansAmountByDay

UNION

SELECT 'G_TotalLoans' AS 'Key', CAST(sum(Amount) AS DECIMAL(18,6)) AS Value  FROM vw_LoansAmountByDay

UNION

SELECT 'G_AvgNewLoan' AS 'Key', sum(LoanAmount) / CAST(count(*) AS DECIMAL(18,6)) AS Value FROM Loan WHERE [Date]>=@MonthAgo AND CustomerId IN (
	SELECT c.Id
	FROM Loan l JOIN Customer c ON l.CustomerId=c.Id 
	JOIN CustomerStatuses s ON s.Id = c.CollectionStatus
	WHERE c.IsTest=0
	AND s.IsDefault=0
	GROUP BY c.Id
	HAVING count(l.Id)=1
)	
UNION

SELECT 'G_AvgLoanSize' AS 'Key', sum(l.LoanAmount) / CAST(count(*) AS DECIMAL(18,6)) AS Value FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE c.IsTest=0

UNION

SELECT 'G_BookSize' AS 'Key', CAST(@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose AS DECIMAL(18,6)) AS Value 

UNION

SELECT 'T_Registration' AS 'Key', CAST(COALESCE(count(c.Id),0) AS DECIMAL(18,6)) AS Value  FROM Customer c WHERE datediff(day, c.GreetingMailSentDate, @Now) = 0 AND c.IsTest=0

UNION

SELECT 'T_Application' AS 'Key', CAST(COALESCE(count(c.Id),0) AS DECIMAL(18,6)) AS Value  FROM Customer c WHERE datediff(day, c.GreetingMailSentDate, @Now) = 0 AND c.IsTest=0 AND c.WizardStep=4

UNION

SELECT 'T_Approved' AS 'Key', CAST(COALESCE(sum(cr.ManagerApprovedSum),0) AS DECIMAL(18,6)) AS Value 
FROM Customer c JOIN CashRequests cr ON c.Id=cr.IdCustomer 
WHERE datediff(day, cr.UnderwriterDecisionDate, @Now) = 0 AND c.IsTest=0 AND UnderwriterDecision='Approved'

UNION

SELECT 'T_LoansOut' AS 'Key', CAST(COALESCE(sum(l.LoanAmount),0) AS DECIMAL(18,6)) AS Value  FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE datediff(day, l.[Date], @Now) = 0 AND c.IsTest=0

UNION

SELECT 'T_Repayments' AS 'Key', CAST(COALESCE(sum(t.Amount),0) AS DECIMAL(18,6)) AS Value  
FROM LoanTransaction t JOIN Loan l ON t.LoanId = l.Id 
JOIN Customer c ON l.CustomerId=c.Id 
WHERE datediff(day, t.PostDate, @Now) = 0 
AND c.IsTest=0
AND t.Type='PaypointTransaction'
AND t.Status='Done'

UNION

SELECT 'M_Registration' AS 'Key', CAST(COALESCE(count(c.Id),0) AS DECIMAL(18,6)) AS Value  FROM Customer c WHERE c.GreetingMailSentDate >= @FirstOfMonth AND c.IsTest=0

UNION

SELECT 'M_Application' AS 'Key', CAST(COALESCE(count(c.Id),0) AS DECIMAL(18,6)) AS Value  FROM Customer c WHERE c.GreetingMailSentDate >= @FirstOfMonth AND c.IsTest=0 AND c.WizardStep=4

UNION

SELECT 'M_Approved' AS 'Key', CAST(COALESCE(sum(x.Value),0) AS DECIMAL(18,6)) AS Value 
FROM (
SELECT max(cr.ManagerApprovedSum) AS Value 
FROM Customer c JOIN CashRequests cr ON c.Id=cr.IdCustomer 
WHERE cr.CreationDate >= @FirstOfMonth AND c.IsTest=0 AND UnderwriterDecision='Approved'
GROUP BY cr.IdCustomer
) x

UNION

SELECT 'M_LoansOut' AS 'Key', CAST(COALESCE(sum(l.LoanAmount),0) AS DECIMAL(18,6)) AS Value  FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE l.[Date] >= @FirstOfMonth AND c.IsTest=0

UNION

SELECT 'M_Repayments' AS 'Key', CAST(COALESCE(sum(t.Amount),0) AS DECIMAL(18,6)) AS Value  
FROM LoanTransaction t JOIN Loan l ON t.LoanId = l.Id 
JOIN Customer c ON l.CustomerId=c.Id 
WHERE t.PostDate >= @FirstOfMonth 
AND c.IsTest=0
AND t.Type='PaypointTransaction'
AND t.Status='Done'

UNION

SELECT 'M_AvgLoanSize' AS 'Key', COALESCE(sum(l.LoanAmount) / CAST(count(*) AS DECIMAL(18,6)),0) AS Value FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE l.[Date] >= @FirstOfMonth AND c.IsTest=0

UNION

SELECT 'M_AvgDailyLoans' AS 'Key', COALESCE(sum(Amount) / CAST(datepart(d,@Now) AS DECIMAL(18,6)),0) AS Value  FROM vw_LoansAmountByDay WHERE [Date] >= @FirstOfMonth

UNION

SELECT 'M_AvgInterest' AS 'Key', COALESCE(sum(l.InterestRate) / CAST(count(*) AS DECIMAL(18,6)),0) AS Value FROM Loan l JOIN Customer c ON l.CustomerId=c.Id WHERE l.[Date] >= @FirstOfMonth AND c.IsTest=0

UNION

SELECT 'T_UkVisitors' AS 'Key', CAST(COALESCE(0,0) AS DECIMAL(18,6)) AS Value -- from ga api

UNION

SELECT 'M_UkVisitors' AS 'Key', CAST(COALESCE(0,0) AS DECIMAL(18,6)) AS Value -- from ga api

END 

GO

