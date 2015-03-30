SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadCashRequestsForAutomationReport') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCashRequestsForAutomationReport AS SELECT 1')
GO

ALTER PROCEDURE LoadCashRequestsForAutomationReport
@CustomerID INT,
@DateFrom DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		r.Id AS CashRequestID,
		r.IdCustomer AS CustomerID,
		c.BrokerID,
		CASE
			WHEN r.IdUnderwriter IS NULL THEN r.SystemDecisionDate
			ELSE r.UnderwriterDecisionDate
		END AS DecisionTime,
		CASE
			WHEN (r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Rejected') THEN CONVERT(BIT, 0)
			WHEN (r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision IN ('Approved', 'ApprovedPending')) THEN CONVERT(BIT, 1)
			WHEN (r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve') THEN CONVERT(BIT, 1)
		END AS IsApproved,
		ISNULL(CASE
			WHEN r.IdUnderwriter IS NULL
				THEN CASE
					WHEN r.UnderwriterComment = 'Auto Re-Approval' THEN r.ManagerApprovedSum
					ELSE r.SystemCalculatedSum
				END
			ELSE
				ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum)
		END, 0) AS ApprovedAmount,
		r.InterestRate,
		ISNULL(r.ApprovedRepaymentPeriod, r.RepaymentPeriod) AS RepaymentPeriod,
		r.UseSetupFee,
		r.UseBrokerSetupFee,
		r.ManualSetupFeePercent,
		r.ManualSetupFeeAmount,
		r.MedalType AS MedalName,
		r.ScorePoints AS EzbobScore,
		ISNULL((
			SELECT COUNT(*)
			FROM Loan
			WHERE CustomerID = r.IdCustomer
			AND [Date] < r.UnderwriterDecisionDate
		), 0) AS PreviousLoanCount,
		ISNULL((
			SELECT COUNT(*)
			FROM Loan
			WHERE RequestCashId = r.Id
		), 0) AS CrLoanCount,
		cs.IsDefault
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
	WHERE
		r.CreationDate >= ISNULL(@DateFrom, 'Sep 4 2012')
		AND
		(r.IdUnderwriter IS NULL OR r.IdUnderwriter != 1)
		AND (
			(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Rejected')
			OR (
				(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision IN ('Approved', 'ApprovedPending'))
				OR
				(r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve')
			)
		)
		AND
		(@CustomerID IS NULL OR r.IdCustomer = @CustomerID)
	ORDER BY
		r.IdCustomer ASC,
		r.UnderwriterDecisionDate ASC,
		r.Id ASC
END
GO
