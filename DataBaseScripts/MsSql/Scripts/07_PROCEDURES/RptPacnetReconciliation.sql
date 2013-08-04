IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptPacnetReconciliation]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptPacnetReconciliation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptPacnetReconciliation
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @Date DATE
	DECLARE @EzbobOut DECIMAL(18, 2)
	DECLARE @PacnetOut DECIMAL(18, 2)
	DECLARE @EzbobIn DECIMAL(18, 2)
	DECLARE @PacnetIn DECIMAL(18, 2)

	DECLARE @EzbobOutCount INT
	DECLARE @PacnetOutCount INT
	DECLARE @EzbobInCount INT
	DECLARE @PacnetInCount INT

	DECLARE @Amount DECIMAL(18, 2), @IsCredit BIT, @EzbobCount INT, @PacnetCount INT

	SELECT @Date = CONVERT(DATE, @DateStart)

	CREATE TABLE #pacnet (
		Amount DECIMAL(18, 2) NOT NULL,
		IsCredit BIT NOT NULL,
		Counter INT NOT NULL
	)

	CREATE TABLE #ezbob (
		Amount DECIMAL(18, 2) NOT NULL,
		IsCredit BIT NOT NULL,
		Counter INT NOT NULL
	)

	CREATE TABLE #res (
		Amount DECIMAL(18, 2) NOT NULL,
		IsCredit BIT NOT NULL,
		EzbobCount INT NOT NULL,
		PacnetCount INT NOT NULL
	)

	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(1000) NOT NULL,
		EzbobAmount DECIMAL(18, 2) NULL,
		EzbobCount INT NULL,
		PacnetAmount DECIMAL(18, 2) NULL,
		PacnetCount INT NULL,
		TransactionID INT NULL,
		Css NVARCHAR(128) NULL
	)

	INSERT INTO #pacnet
	SELECT
		Amount,
		IsCredit,
		COUNT(*)
	FROM
		PacNetBalance
	WHERE
		CONVERT(DATE, Date) = @Date
	GROUP BY
		Amount,
		IsCredit

	INSERT INTO #ezbob
	SELECT
		Amount,
		0,
		COUNT(*)
	FROM
		LoanTransaction
	WHERE
		Type = 'PacnetTransaction'
		AND
		Status = 'Done'
		AND
		CONVERT(DATE, PostDate) = @Date
	GROUP BY
		Amount

	INSERT INTO #res
	SELECT
		e.Amount,
		e.IsCredit,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)
	FROM
		#ezbob e
		LEFT JOIN #pacnet p ON e.Amount = p.Amount AND e.IsCredit = p.IsCredit

	INSERT INTO #res
	SELECT
		p.Amount,
		p.IsCredit,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)
	FROM
		#ezbob e
		RIGHT JOIN #pacnet p ON e.Amount = p.Amount AND e.IsCredit = p.IsCredit
	WHERE
		e.Amount IS NULL

	DELETE FROM #res WHERE EzbobCount = PacnetCount

	SELECT
		@PacnetIn = ISNULL(SUM(ISNULL(Amount, 0)), 0),
		@PacnetInCount = ISNULL(COUNT(*), 0)
	FROM
		#pacnet
	WHERE
		IsCredit = 1

	SELECT
		@PacnetOut = ISNULL(SUM(ISNULL(Amount, 0)), 0),
		@PacnetOutCount = ISNULL(COUNT(*), 0)
	FROM
		#pacnet
	WHERE
		IsCredit = 0

	SELECT
		@EzbobOut = ISNULL(SUM(ISNULL(Amount, 0)), 0),
		@EzbobOutCount = ISNULL(COUNT(*), 0)
	FROM
		#ezbob
	WHERE
		IsCredit = 0

	SELECT
		@EzbobIn = ISNULL(SUM(ISNULL(Amount, 0)), 0),
		@EzbobInCount = ISNULL(COUNT(*), 0)
	FROM
		#ezbob
	WHERE
		IsCredit = 1

	INSERT INTO #out (Caption, EzbobAmount, EzbobCount, PacnetAmount, PacnetCount, Css)
		VALUES ('Total In', @EzbobIn, @EzbobInCount, @PacnetIn, @PacnetInCount, 'total' + CASE WHEN @EzbobIn = @PacnetIn THEN '' ELSE ' unmatched' END)

	DECLARE cur CURSOR FOR
		SELECT Amount, IsCredit, EzbobCount, PacnetCount
		FROM #res
		WHERE IsCredit = 1
		ORDER BY Amount, IsCredit

	OPEN cur

	FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #out (Caption, EzbobAmount, PacnetAmount, Css)
			VALUES (
				'Unmatched ' +
				(CASE @IsCredit WHEN 1 THEN 'credit' ELSE 'debit' END) +
				' ' + CONVERT(NVARCHAR, @Amount),
				@EzbobCount,
				@PacnetCount,
				'unmatched'
			)

		INSERT INTO #out(Caption, TransactionID)
		SELECT
			'Transaction',
			t.Id
		FROM
			LoanTransaction t
		WHERE
			CONVERT(DATE, t.PostDate) = @Date
			AND
			t.Status = 'Done'
			AND
			t.Type = 'PacnetTransaction'
			AND
			t.Amount = @Amount

		FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount
	END

	CLOSE cur
	DEALLOCATE cur

	INSERT INTO #out (Caption, EzbobAmount, EzbobCount, PacnetAmount, PacnetCount, Css)
		VALUES ('Total Out', @EzbobOut, @EzbobOutCount, @PacnetOut, @PacnetOutCount, 'total' + CASE WHEN @EzbobOut = @PacnetOut THEN '' ELSE ' unmatched' END)

	DECLARE cur CURSOR FOR
		SELECT Amount, IsCredit, EzbobCount, PacnetCount
		FROM #res
		WHERE IsCredit = 0
		ORDER BY Amount, IsCredit

	OPEN cur

	FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #out (Caption, EzbobAmount, PacnetAmount, Css)
			VALUES (
				'Unmatched ' +
				(CASE @IsCredit WHEN 1 THEN 'credit' ELSE 'debit' END) +
				' ' + CONVERT(NVARCHAR, @Amount),
				@EzbobCount,
				@PacnetCount,
				'unmatched'
			)

		INSERT INTO #out(Caption, TransactionID)
		SELECT
			'Transaction',
			t.Id
		FROM
			LoanTransaction t
		WHERE
			CONVERT(DATE, t.PostDate) = @Date
			AND
			t.Status = 'Done'
			AND
			t.Type = 'PacnetTransaction'
			AND
			t.Amount = @Amount

		FETCH NEXT FROM cur INTO @Amount, @IsCredit, @EzbobCount, @PacnetCount
	END

	CLOSE cur
	DEALLOCATE cur

	SELECT
		o.SortOrder,
		o.Caption,
		o.EzbobAmount,
		o.EzbobCount,
		o.PacnetAmount,
		o.PacnetCount,
		t.Id,
		t.PostDate,
		t.LoanId,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
		t.Description,
		o.Css
	FROM
		#out o
		LEFT JOIN LoanTransaction t ON o.TransactionID = t.Id
		LEFT JOIN Loan l ON t.LoanId = l.Id
		LEFT JOIN Customer c ON l.CustomerId = c.Id
	ORDER BY
		SortOrder

	DROP TABLE #out
	DROP TABLE #res
	DROP TABLE #ezbob
	DROP TABLE #pacnet
END
GO
