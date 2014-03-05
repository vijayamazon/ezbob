IF OBJECT_ID (N'dbo.GetLowSideGrouped') IS NOT NULL
	DROP FUNCTION dbo.GetLowSideGrouped
GO

CREATE FUNCTION [dbo].[GetLowSideGrouped]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) LowSideAmount, Count(res.IdUnderwriter) LowSided FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected' 
				AND SystemDecision != 'Reject' ) as res
	Group by IdUnderwriter
)

GO

