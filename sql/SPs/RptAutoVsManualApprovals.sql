SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RptAutoVsManualApprovals') IS NULL
	EXECUTE('CREATE PROCEDURE RptAutoVsManualApprovals AS SELECT 1')
GO

ALTER PROCEDURE RptAutoVsManualApprovals
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @Auto NVARCHAR(8) = ' Auto'
	DECLARE @Manual NVARCHAR(8) = 'Manual'
	DECLARE @Total NVARCHAR(8) = '  Total'

	------------------------------------------------------------------------------
	--
	-- Find auto approved cash requests.
	--
	------------------------------------------------------------------------------

	SELECT
		CashRequestID = r.Id
	INTO
		#auto
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	WHERE
		r.AutoDecisionID = 1
		AND
		r.UnderwriterDecision = 'Approved'
		AND
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd

	------------------------------------------------------------------------------
	--
	-- Find manually approved cash requests.
	--
	------------------------------------------------------------------------------

	SELECT
		CashRequestID = r.Id
	INTO
		#manual
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	WHERE
		r.AutoDecisionID IS NULL
		AND
		r.UnderwriterDecision = 'Approved'
		AND
		@DateStart <= r.UnderwriterDecisionDate AND r.UnderwriterDecisionDate < @DateEnd

	------------------------------------------------------------------------------
	--
	-- Create resulting table.
	--
	------------------------------------------------------------------------------

	CREATE TABLE #avm (
		Title NVARCHAR(8),
		ApprovalCount       DECIMAL(18, 6),
		ApprovalCountShare  DECIMAL(18, 6),
		ApprovedAmount      DECIMAL(18, 6),
		ApprovedAmountShare DECIMAL(18, 6),
		LoanCount           DECIMAL(18, 6),
		LoanCountShare      DECIMAL(18, 6),
		LoanAmount          DECIMAL(18, 6),
		LoanAmountShare     DECIMAL(18, 6)
	)

	------------------------------------------------------------------------------

	INSERT INTO #avm (Title,
		ApprovalCount, ApprovalCountShare,
		ApprovedAmount, ApprovedAmountShare,
		LoanCount, LoanCountShare,
		LoanAmount, LoanAmountShare
	) VALUES
		(@Auto,   0, 0, 0, 0, 0, 0, 0, 0),
		(@Manual, 0, 0, 0, 0, 0, 0, 0, 0)

	------------------------------------------------------------------------------
	--
	-- Fill for auto approvals.
	--
	------------------------------------------------------------------------------

	UPDATE #avm SET
		ApprovalCount = (SELECT COUNT(*) FROM #auto)
	WHERE
		Title = @Auto

	------------------------------------------------------------------------------

	UPDATE #avm SET
		ApprovedAmount = ISNULL((
			SELECT SUM(r.ManagerApprovedSum)
			FROM #auto t
			INNER JOIN CashRequests r ON t.CashRequestID = r.Id
		), 0)
	WHERE
		Title = @Auto

	------------------------------------------------------------------------------

	UPDATE #avm SET
		LoanCount = ISNULL(d.Counter, 0),
		LoanAmount = ISNULL(d.Amount, 0)
	FROM (
		SELECT
			Counter = COUNT(*),
			Amount = SUM(l.LoanAmount)
		FROM #auto t
		INNER JOIN Loan l ON t.CashRequestID = l.RequestCashId
		) d
	WHERE
		Title = @Auto

	------------------------------------------------------------------------------
	--
	-- Fill for manual approvals.
	--
	------------------------------------------------------------------------------

	UPDATE #avm SET
		ApprovalCount = (SELECT COUNT(*) FROM #manual)
	WHERE
		Title = @Manual

	------------------------------------------------------------------------------

	UPDATE #avm SET
		ApprovedAmount = ISNULL((
			SELECT SUM(r.ManagerApprovedSum)
			FROM #manual t
			INNER JOIN CashRequests r ON t.CashRequestID = r.Id
		), 0)
	WHERE
		Title = @Manual

	------------------------------------------------------------------------------

	UPDATE #avm SET
		LoanCount = ISNULL(d.Counter, 0),
		LoanAmount = ISNULL(d.Amount, 0)
	FROM (
		SELECT
			Counter = COUNT(*),
			Amount = SUM(l.LoanAmount)
		FROM #manual t
		INNER JOIN Loan l ON t.CashRequestID = l.RequestCashId
		) d
	WHERE
		Title = @Manual

	------------------------------------------------------------------------------
	--
	-- Total.
	--
	------------------------------------------------------------------------------

	INSERT INTO #avm (Title, ApprovalCount, ApprovedAmount, LoanCount, LoanAmount)
	SELECT
		@Total,
		SUM(ApprovalCount),
		SUM(ApprovedAmount),
		SUM(LoanCount),
		SUM(LoanAmount)
	FROM
		#avm

	------------------------------------------------------------------------------
	--
	-- Fill shares.
	--
	------------------------------------------------------------------------------

	UPDATE am SET
		ApprovalCountShare = CASE WHEN t.ApprovalCount = 0 THEN 0 ELSE am.ApprovalCount / t.ApprovalCount END,
		ApprovedAmountShare = CASE WHEN t.ApprovedAmount = 0 THEN 0 ELSE am.ApprovedAmount / t.ApprovedAmount END,
		LoanCountShare = CASE WHEN t.LoanCount = 0 THEN 0 ELSE am.LoanCount / t.LoanCount END,
		LoanAmountShare = CASE WHEN t.LoanAmount = 0 THEN 0 ELSE am.LoanAmount / t.LoanAmount END
	FROM
		#avm am
		INNER JOIN #avm t ON am.Title != @Total AND t.Title = @Total

	------------------------------------------------------------------------------
	--
	-- Output.
	--
	------------------------------------------------------------------------------

	SELECT
		Title = LTRIM(t.Title),
		ApprovalCount = CONVERT(INT, ApprovalCount),
		ApprovalCountShare,
		ApprovedAmount,
		ApprovedAmountShare,
		LoanCount = CONVERT(INT, LoanCount),
		LoanCountShare,
		LoanAmount,
		LoanAmountShare,
		Css = CASE WHEN t.Title = @Total THEN 'total' ELSE '' END
	FROM
		#avm t
	ORDER BY
		t.Title

	------------------------------------------------------------------------------
	--
	-- Clean up
	--
	------------------------------------------------------------------------------

	DROP TABLE #avm
	DROP TABLE #manual
	DROP TABLE #auto
END
GO
