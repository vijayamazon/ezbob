IF OBJECT_ID (N'dbo.GetEscalatedGrouped') IS NOT NULL
	DROP FUNCTION dbo.GetEscalatedGrouped
GO

CREATE FUNCTION [dbo].[GetEscalatedGrouped]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) EscalatedAmount, Count(res.IdUnderwriter) Escalated FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Escalated') as res
	Group by IdUnderwriter
)

GO

