IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPerformencePerMedalReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPerformencePerMedalReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPerformencePerMedalReport]
	@dateStart DateTime, 
	@dateEnd DateTime
AS
BEGIN
	SELECT dbo.Medals.Medal, ISNULL(pr.Processed, 0) as Processed, ISNULL(pr.ProcessedAmount, 0) as ProcessedAmount, 
			ISNULL(pr.MaxTime,0) as MaxTime, ISNULL(pr.AvgTime,0 ) as AvgTime,
			ISNULL(ag.Approved, 0) as Approved, ISNULL(ag.ApprovedAmount,0) as ApprovedAmount, 
			ISNULL(rg.Rejected, 0) as Rejected, ISNULL(rg.RejectedAmount,0) as RejectedAmount, 
			ISNULL(eg.Escalated, 0) as Escalated, ISNULL(eg.EscalatedAmount,0) as EscalatedAmount,
			ISNULL(hg.HighSide, 0) as HighSide, ISNULL(hg.HighSideAmount,0) as HighSideAmount,
			ISNULL(lg.LowSided, 0) as LowSide, ISNULL(lg.LowSideAmount,0) as LowSideAmount,
			ISNULL(pr.LatePayments, 0) as LatePayments, ISNULL(pr.LatePaymentsAmount,0) as LatePaymentsAmount
			FROM 
			dbo.Medals LEFT OUTER JOIN  
	(SELECT dbo.CashRequests.MedalType as Medal, Count(dbo.CashRequests.SystemCalculatedSum) Processed, Sum(dbo.CashRequests.SystemCalculatedSum) ProcessedAmount,
		Max(DateDiff(minute, dbo.CashRequests.SystemDecisionDate, dbo.CashRequests.UnderwriterDecisionDate)) MaxTime, Max(DateDiff(minute, dbo.CashRequests.SystemDecisionDate, dbo.CashRequests.UnderwriterDecisionDate)) AvgTime,
		COUNT(lp.LatePaymentsAmount) as LatePayments, SUM(lp.LatePaymentsAmount) as LatePaymentsAmount
		FROM dbo.CashRequests LEFT OUTER JOIN GetLoanLatePaymentsGrouped() as lp ON dbo.CashRequests.Id = lp.RequestCashId
		Where dbo.CashRequests.SystemDecisionDate >= @dateStart
				AND dbo.CashRequests.UnderwriterDecisionDate <= @dateEnd
		GROUP BY dbo.CashRequests.MedalType) AS pr 
		ON UPPER (dbo.Medals.Medal) = UPPER(pr.Medal)
		LEFT OUTER JOIN GetApprovedGroupedByMedal(@dateStart, @dateEnd) AS ag ON pr.Medal = ag.Medal
		LEFT OUTER JOIN GetRejectedGroupedByMedal(@dateStart, @dateEnd) AS rg ON pr.Medal = rg.Medal
		LEFT OUTER JOIN GetEscalatedGroupedByMedal(@dateStart, @dateEnd) AS eg ON pr.Medal = eg.Medal
		LEFT OUTER JOIN GetHighSideGroupedByMedal(@dateStart, @dateEnd) AS hg ON pr.Medal = hg.Medal
		LEFT OUTER JOIN GetLowSideGroupedByMedal(@dateStart, @dateEnd) AS lg ON pr.Medal = lg.Medal
END
GO
