UPDATE [dbo].[CashRequests]
   SET [RepaymentPeriod] = 3
 WHERE [RepaymentPeriod] = 0
GO