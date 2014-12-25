IF OBJECT_ID('UpdateMpTotalsHmrc') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateMpTotalsHmrc AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateMpTotalsHmrc
@HistoryID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @MpID INT

	EXECUTE GetMarketplaceFromHistoryID 'HMRC', @HistoryID, @MpID OUTPUT

	IF @MpID IS NULL
		RETURN

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	-- 1. Find all "Revenue" field names.

	SELECT
		n.Id
	INTO
		#boxes
	FROM
		MP_VatReturnEntryNames n
	WHERE
		n.Name LIKE '%(Box 6)%'

	-- 2. Select relevant periods only.

	SELECT
		o.DateFrom,
		o.DateTo,
		o.BusinessId,
		MAX(o.Id) AS Id
	INTO
		#periods
	FROM
		MP_VatReturnRecords o
		INNER JOIN Business b
			ON o.BusinessId = b.Id
			AND b.BelongsToCustomer = 1
	WHERE
		ISNULL(o.IsDeleted, 0) = 0
		AND o.CustomerMarketPlaceId = @MpID
	GROUP BY
		o.DateFrom,
		o.DateTo,
		o.BusinessId

	-- 3. Select period and amount for relevant periods only.

	SELECT
		DateFrom = dbo.udfMinDate(o.DateFrom, o.DateTo),
		DateTo   = dbo.udfMaxDate(o.DateFrom, o.DateTo),
		Amount   = i.Amount
	INTO
		#order_items
	FROM
		MP_VatReturnEntries i
		INNER JOIN #periods o ON i.RecordId = o.Id
		INNER JOIN #boxes b ON i.NameId = b.Id
	WHERE
		ISNULL(i.IsDeleted, 0) = 0

	------------------------------------------------------------------------------

	DECLARE @MinDate DATETIME = NULL
	DECLARE @MaxDate DATETIME = NULL

	------------------------------------------------------------------------------

	SELECT
		@MinDate = dbo.udfMonthStart(MIN(DateFrom)),
		@MaxDate = dbo.udfMonthStart(MAX(DateTo))
	FROM
		#order_items

	------------------------------------------------------------------------------

	IF @MinDate IS NULL OR @MaxDate IS NULL
	BEGIN
		DROP TABLE #order_items
		DROP TABLE #periods
		DROP TABLE #boxes

		RETURN
	END

	------------------------------------------------------------------------------
	--
	-- Create temp table for storing results.
	--
	------------------------------------------------------------------------------

	-- Kinda create table
	SELECT
		TheMonth,
		NextMonth = TheMonth,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover
	INTO
		#months
	FROM
		HmrcAggregation
	WHERE
		1 = 0

	------------------------------------------------------------------------------
	--
	-- Extract single months from the relevant transactions.
	--
	------------------------------------------------------------------------------

	DECLARE @CurDate DATETIME = @MinDate

	------------------------------------------------------------------------------

	WHILE @CurDate <= @MaxDate
	BEGIN
		INSERT INTO #months(
			TheMonth,
			NextMonth,
			CustomerMarketPlaceUpdatingHistoryID,
			Turnover
		)
		SELECT
			dbo.udfMonthStart(@CurDate),
			'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value right after the loop.
			@HistoryID,
			0 -- Turnover

		SET @CurDate = DATEADD(month, 1, @CurDate)
	END

	------------------------------------------------------------------------------

	UPDATE #months SET
		NextMonth = DATEADD(second, -1, DATEADD(month, 1, TheMonth))

	------------------------------------------------------------------------------
	--
	-- Calculate Turnover.
	--
	------------------------------------------------------------------------------

	DECLARE m_cur CURSOR FOR
		SELECT TheMonth, NextMonth FROM #months ORDER BY TheMonth

	OPEN m_cur

	------------------------------------------------------------------------------

	DECLARE @NextDate DATETIME

	DECLARE @Turnover NUMERIC(18, 2)

	FETCH NEXT FROM m_cur INTO @CurDate, @NextDate

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT
			@Turnover = SUM(ISNULL(
				i.Amount * dbo.udfDateIntersectionRatio(@CurDate, @NextDate, i.DateFrom, dbo.udfJustBeforeMidnight(i.DateTo)),
				0
			))
		FROM
			#order_items i

		UPDATE #months SET
			Turnover = @Turnover
		WHERE
			TheMonth = @CurDate

		FETCH NEXT FROM m_cur INTO @CurDate, @NextDate
	END

	------------------------------------------------------------------------------

	CLOSE m_cur
	DEALLOCATE m_cur

	------------------------------------------------------------------------------
	--
	-- At this point table #months contains new data.
	--
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	UPDATE HmrcAggregation SET
		IsActive = 0
	FROM
		HmrcAggregation a
		INNER JOIN MP_CustomerMarketPlaceUpdatingHistory h
			ON a.CustomerMarketplaceUpdatingHistoryID = h.Id
			AND h.CustomerMarketplaceID = @MpID
		INNER JOIN #months m ON a.TheMonth = m.TheMonth
	WHERE
		a.IsActive = 1

	------------------------------------------------------------------------------

	INSERT INTO HmrcAggregation (
		TheMonth,
		IsActive,
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover
	FROM
		#months

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------
	--
	-- Clean up.
	--
	------------------------------------------------------------------------------

	DROP TABLE #months
	DROP TABLE #order_items
	DROP TABLE #periods
	DROP TABLE #boxes
END
GO
