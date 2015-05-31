SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadCashRequestsForBravoAutomationReport') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCashRequestsForBravoAutomationReport AS SELECT 1')
GO

ALTER PROCEDURE LoadCashRequestsForBravoAutomationReport
@StartTime DATETIME,
@EndTime DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		CashRequestID = r.Id,
		CustomerID = r.IdCustomer,
		IsApproved = CONVERT(BIT, CASE r.UnderwriterDecision WHEN 'Rejected' THEN 0 ELSE 1 END),	
		r.AutoDecisionID,
		AutoDecisionName = d.DecisionName,
		DecisionTime = r.UnderwriterDecisionDate
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		LEFT JOIN Decisions d ON r.AutoDecisionID = d.DecisionID
	WHERE
		r.CreationDate > ISNULL(@StartTime, 'May 11 2015') -- on May 11 2015 we have released Auto Approve (the last unreleased auto decision)
		AND
		(@EndTime IS NULL OR r.CreationDate < @EndTime)
		AND
		r.UnderwriterDecision IN ('Approved', 'ApprovedPending', 'Rejected')
	ORDER BY
		r.IdCustomer,
		r.UnderwriterDecisionDate
END
GO
