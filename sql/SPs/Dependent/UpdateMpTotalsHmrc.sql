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

	DECLARE @Now DATETIME

	SELECT
		@Now = UpdatingEnd
	FROM
		MP_CustomerMarketPlaceUpdatingHistory
	WHERE
		Id = @HistoryID

	------------------------------------------------------------------------------
	--
	-- Select relevant transactions.
	--
	------------------------------------------------------------------------------

	-- 1. Find all "Revenue" and "Opex" field names.

	CREATE TABLE #boxes (
		Id INT,
		BoxNum INT
	)

	INSERT INTO #boxes (Id, BoxNum)
	SELECT
		n.Id,
		6
	FROM
		MP_VatReturnEntryNames n
	WHERE
		n.Name LIKE '%(Box 6)%'
		OR
		n.Name LIKE '%(Box 8)%'

	INSERT INTO #boxes (Id, BoxNum)
	SELECT
		n.Id,
		7
	FROM
		MP_VatReturnEntryNames n
	WHERE
		n.Name LIKE '%(Box 7)%'
		OR
		n.Name LIKE '%(Box 9)%'

	-- 2. Select all the marketplace periods (entire aggragation data is updated).

	SELECT
		o.DateFrom,
		o.DateTo,
		MAX(o.Id) AS Id
	INTO
		#periods
	FROM
		MP_VatReturnRecords o
		INNER JOIN Business b
			ON o.BusinessId = b.Id
			AND b.BelongsToCustomer = 1
	WHERE
		(
			ISNULL(o.IsDeleted, 0) = 0
			OR
			NOT EXISTS (
				SELECT h.HistoryItemID
				FROM MP_VatReturnRecordDeleteHistory h
				WHERE h.DeletedRecordID = o.Id
				AND h.DeletedTime < @Now
			)
		)
		AND
		o.CustomerMarketPlaceId = @MpID
		AND
		o.CustomerMarketPlaceUpdatingHistoryRecordId <= @HistoryID
	GROUP BY
		o.DateFrom,
		o.DateTo

	-- 3. Select period and amount for relevant periods only.

	;WITH separate_data AS (
		SELECT
			DateFrom = dbo.udfMinDate(o.DateFrom, o.DateTo),
			DateTo   = dbo.udfMaxDate(o.DateFrom, o.DateTo),
			Revenues = CONVERT(DECIMAL(18, 2), i.Amount),
			Opex     = CONVERT(DECIMAL(18, 2), 0)
		FROM
			MP_VatReturnEntries i
			INNER JOIN #periods o ON i.RecordId = o.Id
			INNER JOIN #boxes b
				ON i.NameId = b.Id
				AND b.BoxNum = 6
		WHERE
			ISNULL(i.IsDeleted, 0) = 0
		--
		UNION
		--
		SELECT
			DateFrom = dbo.udfMinDate(o.DateFrom, o.DateTo),
			DateTo   = dbo.udfMaxDate(o.DateFrom, o.DateTo),
			Revenues = CONVERT(DECIMAL(18, 2), 0),
			Opex     = CONVERT(DECIMAL(18, 2), i.Amount)
		FROM
			MP_VatReturnEntries i
			INNER JOIN #periods o ON i.RecordId = o.Id
			INNER JOIN #boxes b
				ON i.NameId = b.Id
				AND b.BoxNum = 7
		WHERE
			ISNULL(i.IsDeleted, 0) = 0
	)
	SELECT
		DateFrom = sd.DateFrom,
		DateTo   = sd.DateTo,
		Revenues = SUM(sd.Revenues),
		Opex     = SUM(sd.Opex)
	INTO
		#order_items
	FROM
		separate_data sd
	GROUP BY
		sd.DateFrom,
		sd.DateTo

	------------------------------------------------------------------------------

	UPDATE #order_items SET Revenues = 0 WHERE Revenues IS NULL
	UPDATE #order_items SET Opex = 0     WHERE Opex     IS NULL

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
		Turnover,
		ValueAdded,
		FreeCashFlow
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
			Turnover,
			ValueAdded,
			FreeCashFlow
		)
		SELECT
			dbo.udfMonthStart(@CurDate),
			'Jul 1 1976', -- Magic number because column ain't no allows null. It is replaced with the real value right after the loop.
			@HistoryID,
			0, -- Turnover
			0, -- ValueAdded
			0  -- FreeCashFlow

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

	DECLARE @Revenues NUMERIC(18, 2)
	DECLARE @Opex NUMERIC(18, 2)

	FETCH NEXT FROM m_cur INTO @CurDate, @NextDate

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT
			@Revenues = SUM(ISNULL(
				i.Revenues * dbo.udfDateIntersectionRatio(@CurDate, @NextDate, i.DateFrom, dbo.udfJustBeforeMidnight(i.DateTo)),
				0
			)),
			@Opex = SUM(ISNULL(
				i.Opex * dbo.udfDateIntersectionRatio(@CurDate, @NextDate, i.DateFrom, dbo.udfJustBeforeMidnight(i.DateTo)),
				0
			))
		FROM
			#order_items i

		UPDATE #months SET
			Turnover = @Revenues,
			ValueAdded = @Revenues - @Opex,
			FreeCashFlow = @Revenues - @Opex
		WHERE
			TheMonth = @CurDate

		FETCH NEXT FROM m_cur INTO @CurDate, @NextDate
	END

	------------------------------------------------------------------------------

	CLOSE m_cur
	DEALLOCATE m_cur

	------------------------------------------------------------------------------
	--
	-- Try to get TotalMonthlySalary FROM CompanyEmployeeCount
	--
	------------------------------------------------------------------------------

	DECLARE @salary NUMERIC(18,2) = NULL

	SELECT TOP 1
		@salary = e.TotalMonthlySalary	
	FROM	
		MP_CustomerMarketPlace AS mp 
		INNER JOIN Customer AS c ON mp.CustomerId = c.Id
		INNER JOIN CompanyEmployeeCount AS e ON e.CustomerId = c.Id
	WHERE
		mp.Id = @MpID
	ORDER BY 
		e.Created DESC

	------------------------------------------------------------------------------
	--
	-- Try to get salaries FROM RtiTaxMonth data
	--
	------------------------------------------------------------------------------

	CREATE TABLE #rti_salaries(	
		DateStart DATETIME,
		Amount NUMERIC(18,2)
	)

	IF @salary IS NULL
	BEGIN
		;WITH raw_rti_salary AS (
			SELECT
				e.DateStart,
				e.AmountPaid,
				RowNum = ROW_NUMBER() OVER (
					PARTITION BY e.DateStart, e.DateEnd
					ORDER BY r.Created DESC, r.CustomerMarketPlaceUpdatingHistoryRecordId DESC
				)
			FROM
				MP_RtiTaxMonthRecords r
				INNER JOIN MP_RtiTaxMonthEntries e ON r.Id = e.RecordId
			WHERE
				r.CustomerMarketPlaceId = @MpID
				AND
				r.Created < @Now
		)
		INSERT INTO #rti_salaries(DateStart, Amount)
		SELECT
			dbo.udfMonthStart(rs.DateStart),
			rs.AmountPaid
		FROM
			raw_rti_salary rs
		WHERE
			rs.RowNum = 1
	END

	------------------------------------------------------------------------------

	IF @salary IS NULL
	BEGIN
		UPDATE #months SET
			FreeCashFlow = m.FreeCashFlow - rs.Amount
		FROM
			#months m
			INNER JOIN #rti_salaries rs ON m.TheMonth = rs.DateStart
	END
	ELSE BEGIN
		UPDATE #months SET
			FreeCashFlow = FreeCashFlow - @salary
	END

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
		Turnover,
		ValueAdded,
		FreeCashFlow
	)
	SELECT
		TheMonth,
		1, -- IsActive
		CustomerMarketPlaceUpdatingHistoryID,
		Turnover,
		ValueAdded,
		FreeCashFlow
	FROM
		#months

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------
	--
	-- Clean up.
	--
	------------------------------------------------------------------------------

	DROP TABLE #rti_salaries
	DROP TABLE #months
	DROP TABLE #order_items
	DROP TABLE #periods
	DROP TABLE #boxes
END
GO
