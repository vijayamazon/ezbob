SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadCashRequestsForAutomationReport') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCashRequestsForAutomationReport AS SELECT 1')
GO

ALTER PROCEDURE LoadCashRequestsForAutomationReport
@RequestedCustomers IntList READONLY,
@DateFrom DATETIME,
@DateTo DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------
	--
	-- Find relevant customers.
	--
	------------------------------------------------------------------------------

	SELECT
		c.Id,
		c.BrokerID,
		c.CollectionStatus
	INTO
		#customers
	FROM
		Customer c
		INNER JOIN @RequestedCustomers rc ON c.Id = rc.Value
	WHERE
		c.IsTest = 0

	------------------------------------------------------------------------------

	IF NOT EXISTS (SELECT Id FROM #customers)
	BEGIN
		INSERT INTO #customers(Id, BrokerId, CollectionStatus)
		SELECT
			c.Id,
			c.BrokerID,
			c.CollectionStatus
		FROM
			Customer c
		WHERE
			c.IsTest = 0
	END

	------------------------------------------------------------------------------
	--
	-- Do the job.
	--
	------------------------------------------------------------------------------

	SELECT
		r.Id AS CashRequestID,
		NLCashRequestID = nlr.CashRequestID,
		r.IdCustomer AS CustomerID,
		c.BrokerID,
		CASE
			WHEN r.IdUnderwriter IS NULL THEN r.SystemDecisionDate
			ELSE r.UnderwriterDecisionDate
		END AS DecisionTime,
		CASE
			WHEN (r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Rejected') THEN CONVERT(BIT, 0)
			WHEN (r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Approved') THEN CONVERT(BIT, 1)
			WHEN (r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve') THEN CONVERT(BIT, 1)
		END AS IsApproved,
		ISNULL(CASE
			WHEN r.ManagerApprovedSum IS NOT NULL THEN r.ManagerApprovedSum
			ELSE CASE
				WHEN r.IdUnderwriter IS NULL THEN CASE
					WHEN r.UnderwriterComment = 'Auto Re-Approval' THEN r.ManagerApprovedSum
					ELSE r.SystemCalculatedSum
				END
				ELSE
					r.SystemCalculatedSum
				END
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
		cs.IsDefault,
		ls.LoanSourceName
	FROM
		CashRequests r
		INNER JOIN #customers c ON r.IdCustomer = c.Id
		INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
		INNER JOIN LoanSource ls ON r.LoanSourceId = ls.LoanSourceID
		LEFT JOIN NL_CashRequests nlr ON nlr.OldCashRequestID = r.Id
	WHERE
		r.CreationDate >= ISNULL(@DateFrom, 'Sep 4 2012')
		AND (
			(CASE
				WHEN r.IdUnderwriter IS NULL THEN r.SystemDecisionDate
				ELSE r.UnderwriterDecisionDate
			END) < ISNULL(@DateTo, 'April 1 2015')
		)
		AND
		(r.IdUnderwriter IS NULL OR r.IdUnderwriter != 1)
		AND (
			(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Rejected')
			OR (
				(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Approved')
				OR
				(r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve')
			)
		)
	ORDER BY
		r.IdCustomer ASC,
		r.UnderwriterDecisionDate ASC,
		r.Id ASC

	------------------------------------------------------------------------------
	--
	-- Clean up.
	--
	------------------------------------------------------------------------------

	DROP TABLE #customers
END
GO
