IF OBJECT_ID (N'dbo.GetApprovedGroupedByMedal') IS NOT NULL
	DROP FUNCTION dbo.GetApprovedGroupedByMedal
GO

CREATE FUNCTION [dbo].[GetApprovedGroupedByMedal]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) ApprovedAmount, Count(res.Medal) Approved FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Approved') as res
	Group by Medal
)

GO

