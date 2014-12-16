IF OBJECT_ID('dbo.udfMonthEnd') IS NOT NULL
	DROP FUNCTION dbo.udfMonthEnd
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfMonthEnd(@d DATETIME)
RETURNS DATETIME
AS
BEGIN
	IF @d IS NULL
		RETURN NULL

	--     3. Subtract one second to get 23:59:59 (or whatever - leap second) of the last day of the requested month.
	--     |                   2. Convert to start of the next month (i. e. 1st of the next month, midnight).
	--     |                   |                 1. Convert to month start (1st of the month, midnight)
	--     |                   |                 |
	RETURN DATEADD(second, -1, DATEADD(month, 1, dbo.udfMonthStart(@d)))
END
GO
