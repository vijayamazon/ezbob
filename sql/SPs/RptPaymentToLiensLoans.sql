IF OBJECT_ID('RptPaymentToLiensLoans') IS NULL
	EXECUTE('CREATE PROCEDURE RptPaymentToLiensLoans AS SELECT 1')
GO

ALTER PROCEDURE RptPaymentToLiensLoans
	 @DateStart DATETIME,
	 @DateEnd DATETIME
AS
BEGIN

SELECT Count(1) Loans, sum(Amount) Amount, W.Name
FROM LoanTransaction T 
JOIN Loan L ON L.Id = T.LoanId
JOIN LoanLien W ON W.Id = L.LoanLienId 
WHERE T.Type = 'PaypointTransaction'
AND T.Status = 'Done' 
AND T.PostDate >= @DateStart 
AND T.PostDate < @DateEnd 
GROUP BY W.Name

END 

