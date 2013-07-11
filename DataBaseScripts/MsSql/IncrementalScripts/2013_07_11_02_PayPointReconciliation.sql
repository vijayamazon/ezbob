IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Recon_Paypoint_Include_Five')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('Recon_Paypoint_Include_Five', 'no', 'Include transactions with amount = 5 in Paypoint reconciliation. Valid values: yes/no. Default: no.')
END
GO

IF OBJECT_ID('RptPaypointReconciliation') IS NOT NULL
	DROP PROCEDURE RptPaypointReconciliation
GO

IF OBJECT_ID('PaypointOneTypeReconciliation') IS NOT NULL
	DROP PROCEDURE PaypointOneTypeReconciliation
GO

CREATE PROCEDURE PaypointOneTypeReconciliation
@Date DATE,
@IncludeFive BIT,
@SuccessOnly BIT
AS
BEGIN
	DECLARE @EzbobTotal DECIMAL(18, 2)
	DECLARE @PaypointTotal DECIMAL(18, 2)

	DECLARE @Amount DECIMAL(18, 2), @EzbobCount INT, @PaypointCount INT
	
	CREATE TABLE #paypoint (
		Amount DECIMAL(18, 2) NOT NULL,
		Counter INT NOT NULL
	)
	
	CREATE TABLE #ezbob (
		Amount DECIMAL(18, 2) NOT NULL,
		Counter INT NOT NULL
	)
	
	CREATE TABLE #res (
		Amount DECIMAL(18, 2) NOT NULL,
		EzbobCount INT NOT NULL,
		PaypointCount INT NOT NULL
	)

	INSERT INTO #paypoint
	SELECT
		amount,
		COUNT(*)
	FROM
		PayPointBalance
	WHERE
		(
			(@SuccessOnly = 1 AND auth_code != '')
			OR
			(@SuccessOnly = 0 AND auth_code = '')
		)
		AND
		CONVERT(DATE, date) = @Date
		AND
		(@IncludeFive = 1 OR Amount != 5)
	GROUP BY
		amount
	
	INSERT INTO #ezbob
	SELECT
		Amount,
		COUNT(*)
	FROM
		LoanTransaction
	WHERE
		Type = 'PaypointTransaction'
		AND
		(
			(@SuccessOnly = 1 AND Status = 'Done')
			OR
			(@SuccessOnly = 0 AND Status != 'Done')
		)
		AND
		CONVERT(DATE, PostDate) = @Date
		AND
		(@IncludeFive = 1 OR Amount != 5)
	GROUP BY
		Amount
	
	INSERT INTO #res
	SELECT
		e.Amount,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)	
	FROM
		#ezbob e
		LEFT JOIN #paypoint p ON e.Amount = p.Amount
	
	INSERT INTO #res
	SELECT
		p.Amount,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)	
	FROM
		#ezbob e
		RIGHT JOIN #paypoint p ON e.Amount = p.Amount
	WHERE
		e.Amount IS NULL

	DELETE FROM #res WHERE EzbobCount = PaypointCount
	
	SELECT
		@PaypointTotal = ISNULL(SUM(Amount), 0)
	FROM
		#paypoint
	
	SELECT
		@EzbobTotal = ISNULL(SUM(Amount), 0)
	FROM
		#ezbob

	INSERT INTO #out (Caption, EzbobAmount, PaypointAmount, Css)
		VALUES ('Total sum', @EzbobTotal, @PaypointTotal, 'total' + CASE WHEN @EzbobTotal = @PaypointTotal THEN '' ELSE ' unmatched' END)

	DECLARE cur CURSOR FOR
		SELECT Amount, EzbobCount, PaypointCount
		FROM #res
		ORDER BY Amount

	OPEN cur

	FETCH NEXT FROM cur INTO @Amount, @EzbobCount, @PaypointCount

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #out (Caption, EzbobAmount, PaypointAmount, Css)
			VALUES (
				'Unmatched debit ' + CONVERT(NVARCHAR, @Amount),
				@EzbobCount,
				@PaypointCount,
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
			(
				(@SuccessOnly = 1 AND t.Status = 'Done')
				OR
				(@SuccessOnly = 0 AND t.Status != 'Done')
			)
			AND
			t.Type = 'PaypointTransaction'
			AND
			t.Amount = @Amount
	
		FETCH NEXT FROM cur INTO @Amount, @EzbobCount, @PaypointCount
	END

	CLOSE cur
	DEALLOCATE cur
	
	DROP TABLE #res
	DROP TABLE #ezbob
	DROP TABLE #paypoint
END
GO

CREATE PROCEDURE RptPaypointReconciliation
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @Date DATE
	DECLARE @IncludeFive BIT

	SELECT @Date = CONVERT(DATE, @DateStart)

	IF EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Recon_Paypoint_Include_Five')
		SET @IncludeFive = CASE (SELECT Value FROM ConfigurationVariables WHERE Name = 'Recon_Paypoint_Include_Five') WHEN 'yes' THEN 1 ELSE 0 END

	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(1000) NOT NULL,
		EzbobAmount DECIMAL(18, 2) NULL,
		PaypointAmount DECIMAL(18, 2) NULL,
		TransactionID INT NULL,
		Css NVARCHAR(128) NULL
	)

	INSERT INTO #out (Caption, Css) VALUES ('Successful Transactions', 'total2')
	INSERT INTO #out (Caption) VALUES ('5-Amount Transactions Are ' + (CASE @IncludeFive WHEN 1 THEN 'Included' ELSE 'Excluded' END))
	
	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 1
	
	INSERT INTO #out (Caption, Css) VALUES ('Failed Transactions', 'total2')
	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 0

	SELECT
		o.SortOrder,
		o.Caption,
		o.EzbobAmount,
		o.PaypointAmount,
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
END
GO
