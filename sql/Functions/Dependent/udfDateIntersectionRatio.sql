IF OBJECT_ID('dbo.udfDateIntersectionRatio') IS NOT NULL
	DROP FUNCTION dbo.udfDateIntersectionRatio
GO

SET QUOTED_IDENTIFIER ON
GO

-- There are two date ranges: [@a, @b] and [@c, @d]. This function returns length of the
-- intersection of these two ranges divided by the length of the second range.
CREATE FUNCTION dbo.udfDateIntersectionRatio(@a DATETIME, @b DATETIME, @c DATETIME, @d DATETIME)
RETURNS NUMERIC(18, 8)
AS
BEGIN
	DECLARE @t DATETIME
	DECLARE @x NUMERIC(18, 8)

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

	IF @b < @c OR @a > @d
		RETURN 0

	------------------------------------------------------------------------------

	IF @a <= @c AND @d <= @b
		RETURN 1

	------------------------------------------------------------------------------

	IF @c <= @a AND @b <= @d
		RETURN dbo.udfDateRangesRatio(@a, @b, @c, @d)

	------------------------------------------------------------------------------

	IF @b <= @d
		SET @x = dbo.udfDateRangesRatio(@c, @b, @c, @d)
	ELSE
		SET @x = dbo.udfDateRangesRatio(@a, @d, @c, @d)

	------------------------------------------------------------------------------

	RETURN @x
END
GO
