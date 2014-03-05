IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetDailyReport]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetDailyReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetDailyReport] 
	(@date Datetime)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT     ROW_NUMBER() OVER(ORDER BY Loan.RefNum) AS Id, query.Date,  Customer.Name as CustomerName,  query.st as Status, query.LoanRepayment AS Paid,
			   query.AmountDue + query.LoanRepayment AS Expected, Loan.Date AS OriginationDate, Loan.LoanAmount,  
			   Loan.Balance as LoanBalance, Loan.RefNum AS LoanRef

	FROM         (SELECT     Date, AmountDue, LoanRepayment, 'EarlyPaid' AS st, loanid
				   FROM          dbo.GetFullyEarlyPaid(@date) AS fullyEarlyPaid
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'Fully paid on time' AS st, loanid
				   FROM         dbo.GetFullyPaidOnTime(@date) AS fullyPaidOnTime
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'partialEarlyPaid' AS st, loanid
				   FROM         dbo.GetPartialEarlyPaid(@date) AS partialEarlyPaid
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'partial Paid On Time' AS st, loanid
				   FROM         dbo.GetPartialPaidOnTime(@date) AS partialPaidOnTime
				   UNION
				   SELECT     Date, AmountDue, LoanRepayment, 'Not Paid' AS st, loanid
				   FROM         dbo.GetNotPaid(@date) AS notPaid
				   UNION
				   SELECT     getdate(), LatePaymentsAmount, 0 , 'Late repayment' AS st, loanid
				   FROM       [dbo].[GetLatePayments] (@date) AS lateRepayment
				   
				   ) AS query 
				   
				   
	INNER JOIN Loan ON Loan.Id = query.loanid 
	INNER JOIN Customer ON Loan.CustomerId = Customer.Id
END
GO
