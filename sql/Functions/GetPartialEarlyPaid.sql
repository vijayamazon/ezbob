IF OBJECT_ID (N'dbo.GetPartialEarlyPaid') IS NOT NULL
	DROP FUNCTION dbo.GetPartialEarlyPaid
GO

CREATE FUNCTION [dbo].[GetPartialEarlyPaid]
(	@date DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT ls.Date, ls.AmountDue, ls.LoanRepayment, ls.loanid FROM dbo.LoanSchedule as ls
	LEFT OUTER JOIN dbo.LoanTransaction as lt ON lt.LoanId = ls.LoanId
	Where ls.Date > @date 
			AND ls.Date < DATEADD(day, 1, @date ) 
			AND lt.Status = 'Done' 
			AND lt.PostDate < @date
			AND ls.AmountDue > 0
			AND ls.LoanRepayment > 0
)

GO

