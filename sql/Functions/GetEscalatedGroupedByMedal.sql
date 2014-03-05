IF OBJECT_ID (N'dbo.GetEscalatedGroupedByMedal') IS NOT NULL
	DROP FUNCTION dbo.GetEscalatedGroupedByMedal
GO

CREATE FUNCTION [dbo].[GetEscalatedGroupedByMedal]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.Medal, SUM(res.SystemCalculatedSum) EscalatedAmount, Count(res.Medal) Escalated FROM 
	(SELECT MedalType as Medal, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Escalated') as res
	Group by Medal
)

GO

