IF OBJECT_ID (N'dbo.GetLatePaymentsGrouped') IS NOT NULL
	DROP FUNCTION dbo.GetLatePaymentsGrouped
GO

CREATE FUNCTION [dbo].[GetLatePaymentsGrouped]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.LoanId, SUM(res.AmountDue) LatePaymentsAmount FROM 
	(SELECT LoanId, AmountDue 
		FROM LoanSchedule
			Where Status = 'Late') as res
	Group by LoanId
)

GO

