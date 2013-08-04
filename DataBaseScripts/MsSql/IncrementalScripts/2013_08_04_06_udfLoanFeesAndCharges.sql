IF OBJECT_ID('dbo.udfLoanFeesAndCharges') IS NOT NULL
	DROP FUNCTION dbo.udfLoanFeesAndCharges
GO

CREATE FUNCTION dbo.udfLoanFeesAndCharges(
	@DateStart DATETIME,
	@DateEnd DATETIME
)
RETURNS @output TABLE (
	LoanID INT NOT NULL,
	Fees DECIMAL(18, 2) NOT NULL,
	Charges DECIMAL(18, 2) NOT NULL
)
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	------------------------------------------------------------------------------

	DECLARE @Fees TABLE (
		LoanID INT NOT NULL,
		Fees DECIMAL(18, 2) NOT NULL
	)

	------------------------------------------------------------------------------

	DECLARE @Charges TABLE (
		LoanID INT NOT NULL,
		Charges DECIMAL(18, 2) NOT NULL
	)

	------------------------------------------------------------------------------

	DECLARE @SetupFee TABLE (
		LoanID INT NOT NULL,
		SetupFee DECIMAL(18, 2) NOT NULL
	)

	------------------------------------------------------------------------------

	INSERT INTO @Fees
	SELECT
		t.LoanId,
		SUM(t.Fees)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
	WHERE
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
	GROUP BY
		t.LoanID
	
	------------------------------------------------------------------------------

	INSERT INTO @Charges
	SELECT
		c.LoanId,
		CONVERT(DECIMAL(18, 2),
			SUM(CASE
				WHEN ISNULL(AmountPaid, 0) > 0 THEN
					CASE WHEN ISNULL(AmountPaid, 0) < ISNULL(Amount, 0) THEN ISNULL(AmountPaid, 0) ELSE Amount END
				ELSE 0
			END)
		)
	FROM
		LoanCharges c
		INNER JOIN Loan l ON c.LoanId = l.Id
	WHERE
		@DateStart <= c.Date AND c.Date < @DateEnd
	GROUP BY
		c.LoanID

	------------------------------------------------------------------------------

	INSERT INTO @SetupFee
	SELECT
		t.LoanId,
		SUM(t.Fees)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
	WHERE
		t.Type = 'PacnetTransaction'
		AND
		t.Status = 'Done'
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
	GROUP BY
		t.LoanID

	------------------------------------------------------------------------------

	INSERT INTO @output
	SELECT
		ISNULL(f.LoanID, ISNULL(c.LoanID, s.LoanID)),
		ISNULL(f.Fees, 0) + ISNULL(s.SetupFee, 0),
		ISNULL(c.Charges, 0) + ISNULL(s.SetupFee, 0)
	FROM
		@Fees f
		FULL OUTER JOIN @Charges c ON f.LoanID = c.LoanID
		FULL OUTER JOIN @SetupFee s
			ON f.LoanID = s.LoanID
			OR c.LoanID = s.LoanID
	WHERE
		ISNULL(f.Fees, 0) + ISNULL(s.SetupFee, 0) > 0
		OR
		ISNULL(c.Charges, 0) + ISNULL(s.SetupFee, 0) > 0
	
	RETURN
END	
GO

