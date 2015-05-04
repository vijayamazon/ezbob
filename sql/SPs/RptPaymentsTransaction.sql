IF OBJECT_ID('RptPaymentsTransaction') IS NULL
	EXECUTE('CREATE PROCEDURE RptPaymentsTransaction AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptPaymentsTransaction
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
	   T.Id AS TransactionsID,
	   T.PostDate AS TransactionDate,
	   T.LoanRepayment,
	   T.Interest,
	   T.Fees,
	   T.LoanId,
	   L.[Date] AS LoanDate,
	   T.PaypointId,
	   M.Name AS TransactionType,
	   PPA.Mid AS OrangeMoneyAccount,
	   DATEADD(MONTH,DATEDIFF(MONTH, 0,T.PostDate), 0) AS TransactionMonth
	FROM
		LoanTransaction T
		INNER JOIN loan L ON L.Id = T.LoanId
		INNER JOIN LoanTransactionMethod M ON M.Id = T.LoanTransactionMethodId
		INNER JOIN Customer c ON L.CustomerId = c.Id AND c.IsTest = 0
		LEFT JOIN PayPointCard PP ON T.PaypointId LIKE PP.TransactionId + '%'
		LEFT JOIN PayPointAccount PPA ON PPA.PayPointAccountID = PP.PayPointAccountID
	WHERE
		T.Type = 'PaypointTransaction'
		AND
		T.Status = 'Done'
		AND
		@DateStart <= T.PostDate AND T.PostDate < @DateEnd
END
GO
