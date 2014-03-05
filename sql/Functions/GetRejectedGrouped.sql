IF OBJECT_ID (N'dbo.GetRejectedGrouped') IS NOT NULL
	DROP FUNCTION dbo.GetRejectedGrouped
GO

CREATE FUNCTION [dbo].[GetRejectedGrouped]
(	@dateStart DateTime, 
	@dateEnd DateTime
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT res.IdUnderwriter, SUM(res.SystemCalculatedSum) RejectedAmount, Count(res.IdUnderwriter) Rejected FROM 
	(SELECT IdUnderwriter, SystemCalculatedSum 
		FROM CashRequests 
			Where SystemDecisionDate >= @dateStart
				AND UnderwriterDecisionDate <= @dateEnd
				AND UnderwriterDecision = 'Rejected') as res
	Group by IdUnderwriter
)

GO

