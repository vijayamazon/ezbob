IF OBJECT_ID (N'dbo.GetRejectedGroupedByMedal') IS NOT NULL
	DROP FUNCTION dbo.GetRejectedGroupedByMedal
GO

CREATE FUNCTION [dbo].[GetRejectedGroupedByMedal]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) RejectedAmount, Count(res.Medal) Rejected FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected') as res
	Group by Medal
)

GO

