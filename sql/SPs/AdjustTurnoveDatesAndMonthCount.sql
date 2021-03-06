IF OBJECT_ID('AdjustTurnoveDatesAndMonthCount') IS NULL
	EXECUTE('CREATE PROCEDURE AdjustTurnoveDatesAndMonthCount AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AdjustTurnoveDatesAndMonthCount
@MpID INT,
@MonthCount INT OUTPUT,
@DateTo DATETIME OUTPUT,
@DateFrom DATETIME OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	IF @MonthCount IS NULL
		SET @MonthCount = 1

	SET @MonthCount = ABS(@MonthCount)

	IF @MonthCount = 0
		SET @MonthCount = 1

	------------------------------------------------------------------------------

	IF @DateTo IS NOT NULL
	BEGIN
		SELECT
			@DateTo = MAX(m.UpdatingEnd)
		FROM
			MP_CustomerMarketPlace m
		WHERE
			m.Id = @MpID
			AND
			m.UpdatingEnd < @DateTo
	END

	IF @DateTo IS NULL
	BEGIN
		SELECT
			@DateTo = m.UpdatingEnd
		FROM
			MP_CustomerMarketPlace m
		WHERE
			m.Id = @MpID
	END

	IF @DateTo IS NULL
		SET @DateTo = GETUTCDATE()

	-- Convert @DateTo's time part to 23:59:59 without changing a date part
	SET @DateTo = DATEADD(second, -1, DATEADD(day, 1, CONVERT(DATETIME, CONVERT(DATE, @DateTo))))

	------------------------------------------------------------------------------

	IF @MonthCount = 1
	BEGIN
		SET @DateFrom = DATEADD(month, -1, @DateTo)
	END
	ELSE BEGIN
		IF DATEPART(day, @DateTo) = 1
		BEGIN
			SET @DateFrom = DATEADD(month, -@MonthCount, @DateTo)
			SET @DateTo = DATEADD(day, -1, @DateTo)
		END
		ELSE BEGIN
			SET @DateFrom = DATEADD(month, -(@MonthCount - 1), @DateTo)
			SET @DateFrom = DATEADD(month, DATEDIFF(month, 0, @DateFrom), 0)
		END
	END

	-- Convert @DateFrom's time part to 0:00:00 without changing a date part
	SET @DateFrom = CONVERT(DATETIME, CONVERT(DATE, @DateFrom))
END
GO
