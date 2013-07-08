IF OBJECT_ID('RptPacnetReconciliation') IS NOT NULL
	DROP PROCEDURE RptPacnetReconciliation
GO

CREATE PROCEDURE RptPacnetReconciliation
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @Date DATE
	DECLARE @EzbobTotal DECIMAL(18, 2)
	DECLARE @PacnetTotal DECIMAL(18, 2)

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
		PacnetAmount DECIMAL(18, 2) NULL,
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
		@PacnetTotal = ISNULL(SUM(Amount * (1 - 2 * CAST(IsCredit AS INT))), 0)
	FROM
		#pacnet
	
	SELECT
		@EzbobTotal = ISNULL(SUM(Amount * (1 - 2 * CAST(IsCredit AS INT))), 0)
	FROM
		#ezbob

	INSERT INTO #out (Caption, EzbobAmount, PacnetAmount, Css)
		VALUES ('Total sum', @EzbobTotal, @PacnetTotal, 'total' + CASE WHEN @EzbobTotal = @PacnetTotal THEN '' ELSE ' unmatched' END)

	DECLARE cur CURSOR FOR
		SELECT Amount, IsCredit, EzbobCount, PacnetCount
		FROM #res
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
		o.PacnetAmount,
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

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_PACNET_RECONCILIATION')
BEGIN
	INSERT INTO ReportScheduler VALUES('RPT_PACNET_RECONCILIATION', 'Pacnet Reconciliation', 'RptPacnetReconciliation', 0, 0, 0,
		'ID,Caption,Ezbob Amount,Pacnet Amount,Transaction ID,Transaction Time,Client ID,Loan ID,Client Name,Client Email,Description,Css',
		'!SortOrder,Caption,EzbobAmount,PacnetAmount,!Id,PostDate,!ClientID,!LoanID,ClientName,ClientEmail,Description,{Css',
		'alexbo+rpt@ezbob.com', 0)
END
ELSE BEGIN
	UPDATE ReportScheduler SET
		Title = 'Pacnet Reconciliation',
		StoredProcedure = 'RptPacnetReconciliation',
		IsDaily = 0,
		IsWeekly = 0,
		IsMonthly = 0,
		Header = 'ID,Caption,Ezbob Amount,Pacnet Amount,Transaction ID,Transaction Time,Client ID,Loan ID,Client Name,Client Email,Description,Css',
		Fields = '!SortOrder,Caption,EzbobAmount,PacnetAmount,!Id,PostDate,!ClientID,!LoanID,ClientName,ClientEmail,Description,{Css',
		IsMonthToDate = 0
	WHERE
		Type = 'RPT_PACNET_RECONCILIATION'
END
GO
