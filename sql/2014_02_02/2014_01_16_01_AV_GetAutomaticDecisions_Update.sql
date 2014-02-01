ALTER PROCEDURE AV_GetAutomaticDecisions
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
		SET @DateStart = CONVERT(DATE, @DateStart)
		SET @DateEnd = CONVERT(DATE, @DateEnd)
		
		SELECT cr.Id CashRequestId, cr.IdCustomer CustomerId, cr.SystemDecision, cr.SystemDecisionDate, cr.SystemCalculatedSum, cr.ManagerApprovedSum, cr.MedalType, cr.RepaymentPeriod, cr.ScorePoints, cr.ExpirianRating, cr.AnualTurnover, cr.InterestRate, cr.HasLoans, d.Comment FROM CashRequests cr
		LEFT OUTER JOIN DecisionHistory d
		ON cr.Id = d.CashRequestId
		WHERE cr.SystemDecisionDate BETWEEN @DateStart AND @DateEnd
		AND (d.UnderwriterId = 1)
		UNION
		SELECT cr.Id CashRequestId, cr.IdCustomer CustomerId, cr.SystemDecision, cr.SystemDecisionDate, cr.SystemCalculatedSum, 0 AS ManagerApprovedSum, cr.MedalType, cr.RepaymentPeriod, cr.ScorePoints, cr.ExpirianRating, cr.AnualTurnover, cr.InterestRate, cr.HasLoans, '' AS Comment FROM CashRequests cr
		WHERE cr.SystemDecisionDate >= @DateStart AND cr.SystemDecisionDate < @DateEnd
		AND cr.SystemDecision = 'Manual'
	
END
GO

