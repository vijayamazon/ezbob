IF OBJECT_ID (N'dbo.GetNotPaid') IS NOT NULL
	DROP FUNCTION dbo.GetNotPaid
GO

CREATE FUNCTION [dbo].[GetNotPaid]
(	@date DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ls.Date, ls.AmountDue, ls.LoanRepayment, ls.loanid FROM dbo.LoanSchedule as ls
	Where ls.Date > @date 
			AND ls.Date < DATEADD(day, 1, @date ) 
			AND ls.Date >= @date
			AND ls.AmountDue > 0
			AND ls.LoanRepayment = 0
)

GO

