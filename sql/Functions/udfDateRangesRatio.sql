IF OBJECT_ID('dbo.udfDateRangesRatio') IS NOT NULL
	DROP FUNCTION dbo.udfDateRangesRatio
GO

SET QUOTED_IDENTIFIER ON
GO

-- There are two date ranges: [@a, @b] and [@c, @d]. This function returns length of the
-- first range divided by the length of the second range. Range length is taken in seconds.
CREATE FUNCTION dbo.udfDateRangesRatio(@a DATETIME, @b DATETIME, @c DATETIME, @d DATETIME)
RETURNS NUMERIC(18, 8)
AS
BEGIN
	DECLARE @t DATETIME

	------------------------------------------------------------------------------

	IF @a IS NULL OR @b IS NULL OR @c IS NULL OR @d IS NULL
		RETURN 0

	------------------------------------------------------------------------------

	IF @a = @b OR @c = @d
		RETURN 0

	------------------------------------------------------------------------------

	IF @b < @a
	BEGIN
		SET @t = @a
		SET @a = @b
		SET @b = @t
	END

	------------------------------------------------------------------------------

	IF @d < @c
	BEGIN
		SET @t = @c
		SET @c = @d
		SET @d = @t
	END

	------------------------------------------------------------------------------

	RETURN
		CONVERT(NUMERIC(18, 8), DATEDIFF(second, @a, @b)) /
		CONVERT(NUMERIC(18, 8), DATEDIFF(second, @c, @d))
END
GO
