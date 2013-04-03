IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFullyEarlyPaid]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetFullyEarlyPaid]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetFullyEarlyPaid]
(	
	@date DateTime
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
			AND ls.AmountDue = 0
			AND ls.LoanRepayment > 0
)
GO
