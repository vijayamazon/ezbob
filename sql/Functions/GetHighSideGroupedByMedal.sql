IF OBJECT_ID (N'dbo.GetHighSideGroupedByMedal') IS NOT NULL
	DROP FUNCTION dbo.GetHighSideGroupedByMedal
GO

CREATE FUNCTION [dbo].[GetHighSideGroupedByMedal]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) HighSideAmount, Count(res.Medal) HighSide FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved' 
				AND SystemDecision != 'Approve' ) as res
	Group by Medal
)

GO

