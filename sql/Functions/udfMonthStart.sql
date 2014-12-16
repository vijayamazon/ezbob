IF OBJECT_ID('dbo.udfMonthStart') IS NOT NULL
	DROP FUNCTION dbo.udfMonthStart
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfMonthStart(@d DATETIME)
RETURNS DATETIME
AS
BEGIN
	IF @d IS NULL
		RETURN @d

	--     4. Restore ability to have time part.
	--     |                 3. Add one day - convert to the first day of the original month.
	--     |                 |               2. Convert to the last day of the previous month.
	--     |                 |               |                                1. Drop time part.
	--     |                 |               |                                |
	RETURN CONVERT(DATETIME, DATEADD(day, 1, DATEADD(day, -DATEPART(day, @d), CONVERT(DATE, @d))))

END
GO
