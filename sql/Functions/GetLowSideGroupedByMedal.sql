IF OBJECT_ID (N'dbo.GetLowSideGroupedByMedal') IS NOT NULL
	DROP FUNCTION dbo.GetLowSideGroupedByMedal
GO

CREATE FUNCTION [dbo].[GetLowSideGroupedByMedal]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) LowSideAmount, Count(res.Medal) LowSided FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected' 
				AND SystemDecision != 'Reject' ) as res
	Group by Medal
)

GO

