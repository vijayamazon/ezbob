IF OBJECT_ID (N'dbo.GetLatePayments') IS NOT NULL
	DROP FUNCTION dbo.GetLatePayments
GO

CREATE FUNCTION [dbo].[GetLatePayments]
(	@date DateTime
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

