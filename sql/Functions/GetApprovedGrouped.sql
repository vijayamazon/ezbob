IF OBJECT_ID (N'dbo.GetApprovedGrouped') IS NOT NULL
	DROP FUNCTION dbo.GetApprovedGrouped
GO

CREATE FUNCTION [dbo].[GetApprovedGrouped]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) ApprovedAmount, Count(res.IdUnderwriter) Approved FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved') as res
	Group by IdUnderwriter
)

GO

