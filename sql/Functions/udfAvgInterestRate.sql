IF OBJECT_ID('dbo.udfAvgInterestRate') IS NOT NULL
	DROP FUNCTION dbo.udfAvgInterestRate
GO

CREATE FUNCTION dbo.udfAvgInterestRate(@DateEnd DATETIME)
RETURNS DECIMAL(22, 6)
AS
BEGIN
	DECLARE @res DECIMAL(22, 6)
	DECLARE @w DECIMAL(22, 6)
	DECLARE @n DECIMAL(22, 6)

	------------------------------------------------------------------------------

	DECLARE @PACNET NVARCHAR(32) = 'PacnetTransaction'
	DECLARE @PAYPOINT NVARCHAR(32) = 'PaypointTransaction'
	DECLARE @DONE NVARCHAR(4) = 'Done'

	------------------------------------------------------------------------------

	DECLARE @given TABLE (
		LoanID INT,
		Given DECIMAL(22, 6)
	)

	------------------------------------------------------------------------------

	DECLARE @repaid TABLE (
		LoanID INT,
		Repaid DECIMAL(22, 6)
	)

	------------------------------------------------------------------------------

	DECLARE @l TABLE (
		LoanID INT,
		InterestRate DECIMAL(22, 6),
		Weight DECIMAL(22, 6),
		Given DECIMAL(22, 6),
		Repaid DECIMAL(22, 6),
		BadDebt DECIMAL(22, 6),
		Defaults DECIMAL(22, 6)
	)

	------------------------------------------------------------------------------

	INSERT INTO @given(LoanID, Given)
	SELECT
		t.LoanId, ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd
	GROUP BY
		t.LoanId

	------------------------------------------------------------------------------

	INSERT INTO @repaid (LoanID, Repaid)
	SELECT
		t.LoanId, ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd
	GROUP BY
		t.LoanId

	------------------------------------------------------------------------------

	-- No need for full outer join: if nothing given then nothing repaid...

	INSERT INTO @l (LoanID, InterestRate, Weight, Given, Repaid, BadDebt, Defaults)
	SELECT
		g.LoanID,
		0,
		0,
		g.Given,
		r.Repaid,
		0,
		0
	FROM
		@given g
		LEFT JOIN @repaid r ON g.LoanID = r.LoanID

	------------------------------------------------------------------------------

	UPDATE @l	SET
		BadDebt = ISNULL((CASE WHEN l.Status != 'PaidOff' AND s.Name IN ('Bad', 'Debt Management', 'Legal', 'Fraud') THEN l.Principal ELSE 0 END), 0),
		Defaults = ISNULL((CASE WHEN l.Status != 'PaidOff' AND s.Name IN ('Default', 'WriteOff') THEN l.Principal ELSE 0 END), 0)
	FROM
		@l tmp
		INNER JOIN Loan l
			ON tmp.LoanID = l.Id
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
			AND c.IsTest = 0
		LEFT JOIN CustomerStatuses s
			ON c.CollectionStatus = s.Id

	------------------------------------------------------------------------------

	UPDATE @l SET
		InterestRate = l.InterestRate
	FROM
		@l tmp
		INNER JOIN Loan l ON tmp.LoanID = l.Id

	------------------------------------------------------------------------------

	UPDATE @l SET
		Weight = Given - Repaid - BadDebt - Defaults

	------------------------------------------------------------------------------

	SELECT
		@w = ISNULL(SUM(Weight), 0)
	FROM
		@l

	------------------------------------------------------------------------------

	IF @w = 0
		SET @res = 0
	ELSE
	BEGIN
		SELECT
			@n = ISNULL(SUM(Weight * InterestRate), 0)
		FROM
			@l

		SET @res = @n / @w
	END

	------------------------------------------------------------------------------

	RETURN @res
END
GO
