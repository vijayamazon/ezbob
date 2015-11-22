SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RptNotAutoApproved') IS NULL
	EXECUTE('CREATE PROCEDURE RptNotAutoApproved AS SELECT 1')
GO

ALTER PROCEDURE RptNotAutoApproved
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	;WITH cr AS (
		SELECT
			CustomerID = r.IdCustomer,
			CashRequestID = r.Id,
			r.CreationDate,
			r.UnderwriterDecisionDate,
			r.ManagerApprovedSum,
			r.InterestRate,
			r.ApprovedRepaymentPeriod,
			r.ManualSetupFeePercent,
			r.BrokerSetupFeePercent,
			r.SpreadSetupFee
		FROM
			CashRequests r
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		WHERE
			r.UnderwriterDecision = 'Approved'
			AND
			r.AutoDecisionID IS NULL
			AND
			r.UnderwriterDecisionDate BETWEEN @DateStart AND @DateEnd
	)
	SELECT
		cr.CustomerID,
		cr.CashRequestID,
		cr.CreationDate,
		cr.UnderwriterDecisionDate,
		cr.ManagerApprovedSum,
		cr.InterestRate,
		cr.ApprovedRepaymentPeriod,
		cr.ManualSetupFeePercent,
		cr.BrokerSetupFeePercent,
		SpreadSetupFee = ISNULL(cr.SpreadSetupFee, 0),
		t.TrailID,
		t.DecisionTime,
		ds.DecisionStatus,
		tt.TrailTag,
		tc.Position,
		tcn.TraceName,
		tc.Comment
	FROM
		cr
		INNER JOIN DecisionTrail t ON cr.CashRequestID = t.CashRequestID AND t.IsPrimary = 1
		INNER JOIN Decisions d ON t.DecisionID = d.DecisionID AND d.DecisionName = 'Approve'
		INNER JOIN DecisionStatuses ds ON t.DecisionStatusID = ds.DecisionStatusID
		INNER JOIN DecisionTrailTags tt ON t.TrailTagID = tt.TrailTagID
		LEFT JOIN DecisionTrace tc ON t.TrailID = tc.TrailID AND tc.DecisionStatusID != 1
		LEFT JOIN DecisionTraceNames tcn ON tc.TraceNameID = tcn.TraceNameID
	ORDER BY
		cr.CustomerID,
		cr.CashRequestID,
		t.TrailID,
		tc.Position
END
GO
