IF OBJECT_ID (N'dbo.GetHighSideGrouped') IS NOT NULL
	DROP FUNCTION dbo.GetHighSideGrouped
GO

CREATE FUNCTION [dbo].[GetHighSideGrouped]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) HighSideAmount, Count(res.IdUnderwriter) HighSide FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved' 
				AND SystemDecision != 'Approve' ) as res
	Group by IdUnderwriter
)

GO

