SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RptAutoApprovals') IS NULL
	EXECUTE('CREATE PROCEDURE RptAutoApprovals AS SELECT 1')
GO

ALTER PROCEDURE RptAutoApprovals
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		CashRequestID = r.Id,
		CustomerID = r.IdCustomer,
		c.FirstName,
		c.Surname,
		Email = c.Name,
		DecisionTime = r.UnderwriterDecisionDate,
		Amount = r.ManagerApprovedSum,
		Period = r.ApprovedRepaymentPeriod,
		r.InterestRate,
		SetupFeeRate = r.ManualSetupFeePercent,
		MedalAmount = r.SystemCalculatedSum,
		LoanID = l.Id,
		LoanRefNum = l.RefNum,
		l.LoanAmount,
		LoanIssueTime = l.[Date]
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		LEFT JOIN Loan l ON r.Id = l.RequestCashId
	WHERE
		r.AutoDecisionID = 1
		AND
		r.UnderwriterDecision = 'Approved'
		AND
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd
	ORDER BY
		r.UnderwriterDecisionDate
END
GO
