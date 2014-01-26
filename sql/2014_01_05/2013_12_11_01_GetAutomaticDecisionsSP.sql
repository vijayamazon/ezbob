IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetAutomaticDecisions]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetAutomaticDecisions]
GO


CREATE PROCEDURE AV_GetAutomaticDecisions
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
		SET @DateStart = CONVERT(DATE, @DateStart)
		SET @DateEnd = CONVERT(DATE, @DateEnd)
		
		SELECT cr.Id CashRequestId, cr.IdCustomer CustomerId, cr.SystemDecision, cr.SystemDecisionDate, cr.SystemCalculatedSum, cr.MedalType, cr.RepaymentPeriod, cr.ScorePoints, cr.ExpirianRating, cr.AnualTurnover, cr.InterestRate, cr.HasLoans, d.Comment FROM CashRequests cr
		LEFT OUTER JOIN DecisionHistory d
		ON cr.Id = d.CashRequestId
		WHERE cr.SystemDecisionDate BETWEEN @DateStart AND @DateEnd
		AND (d.UnderwriterId = 1)
		UNION
		SELECT cr.Id CashRequestId, cr.IdCustomer CustomerId, cr.SystemDecision, cr.SystemDecisionDate, cr.SystemCalculatedSum, cr.MedalType, cr.RepaymentPeriod, cr.ScorePoints, cr.ExpirianRating, cr.AnualTurnover, cr.InterestRate, cr.HasLoans, '' AS Comment FROM CashRequests cr
		WHERE cr.SystemDecisionDate BETWEEN @DateStart AND @DateEnd
		AND cr.SystemDecision = 'Manual'
	
END
GO
