IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaypointOneTypeReconciliation]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[PaypointOneTypeReconciliation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PaypointOneTypeReconciliation] 
	(@Date DATE,
@IncludeFive BIT,
@SuccessOnly BIT,
@Caption NVARCHAR(64))
AS
BEGIN
	DECLARE @EzbobTotal DECIMAL(18, 2)
	DECLARE @PaypointTotal DECIMAL(18, 2)

	DECLARE @Amount DECIMAL(18, 2), @EzbobCount INT, @PaypointCount INT

	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------

	INSERT INTO #res
	SELECT
		e.Amount,
		ISNULL(e.Counter, 0),
		ISNULL(p.Counter, 0)	
	FROM
		#ezbob e
		LEFT JOIN #paypoint p ON e.Amount = p.Amount

	------------------------------------------------------------------------------

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

	------------------------------------------------------------------------------

	SELECT
		@PaypointTotal = ISNULL(SUM(Amount * Counter), 0)
	FROM
		#paypoint

	------------------------------------------------------------------------------

	SELECT
		@EzbobTotal = ISNULL(SUM(Amount * Counter), 0)
	FROM
		#ezbob

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Amount, Css)
		VALUES (
			'Ezbob Total ' + @Caption + ' Transactions',
			@EzbobTotal,
			@Caption + CASE WHEN @EzbobTotal = @PaypointTotal THEN '' ELSE ' unmatched' END
		)

	------------------------------------------------------------------------------

	INSERT INTO #out (Caption, Amount, Css)
		VALUES ('Paypoint Total ' + @Caption + ' Transactions',
			@PaypointTotal,
			@Caption + CASE WHEN @EzbobTotal = @PaypointTotal THEN '' ELSE ' unmatched' END
		)

	------------------------------------------------------------------------------

	DELETE FROM #res WHERE EzbobCount = PaypointCount

	------------------------------------------------------------------------------

	DECLARE cur CURSOR FOR
		SELECT Amount, EzbobCount, PaypointCount
		FROM #res
		ORDER BY Amount

	OPEN cur

	------------------------------------------------------------------------------

	FETCH NEXT FROM cur INTO @Amount, @EzbobCount, @PaypointCount

	------------------------------------------------------------------------------

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO #out(Caption, TranID)
		SELECT
			'Ezbob',
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

		-----------------------------------------------------------------------------

		INSERT INTO #out(Caption, TranID)
		SELECT
			'Paypoint',
			b.Id
		FROM
			PayPointBalance b
		WHERE
			(
				(@SuccessOnly = 1 AND b.auth_code != '')
				OR
				(@SuccessOnly = 0 AND b.auth_code = '')
			)
			AND
			CONVERT(DATE, b.date) = @Date
			AND
			(@IncludeFive = 1 OR b.Amount != 5)
			AND
			b.amount = @Amount

		-------------------------------------------------------------------------

		FETCH NEXT FROM cur INTO @Amount, @EzbobCount, @PaypointCount
	END

	------------------------------------------------------------------------------

	CLOSE cur
	DEALLOCATE cur

	------------------------------------------------------------------------------

	DROP TABLE #res
	DROP TABLE #ezbob
	DROP TABLE #paypoint
END
GO
