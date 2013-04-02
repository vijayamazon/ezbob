IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLatePayments]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[GetLatePayments]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLatePayments]
(	
  @date DateTime	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.LoanId, SUM(res.AmountDue) LatePaymentsAmount FROM 
	(SELECT LoanId, AmountDue 
		FROM LoanSchedule
			Where Status = 'Late' AND 
			Date > @date and
			Date < DATEADD(day, 1, @date )) as res
	Group by LoanId
)
GO
