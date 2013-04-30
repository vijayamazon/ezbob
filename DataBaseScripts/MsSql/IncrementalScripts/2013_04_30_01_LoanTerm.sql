UPDATE [dbo].[LoanType]
   SET [RepaymentPeriod] = 6
 WHERE [Type] = 'StandardLoanType'
GO