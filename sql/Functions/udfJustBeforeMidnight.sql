IF OBJECT_ID('dbo.udfJustBeforeMidnight') IS NOT NULL
	DROP FUNCTION dbo.udfJustBeforeMidnight
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfJustBeforeMidnight(@d DATETIME)
RETURNS DATETIME
AS
BEGIN
	IF @d IS NULL
		RETURN NULL

	--     4. Subtract one second to move to 23:59:59 (or whatever - leap second) of the previous day.
	--     |                   3. Add one day to move to the midnight of the next day.
	--     |                   |               2. Restore ability to have time part.
	--     |                   |               |                 1. Drop time (move to midnight of the same day).
	RETURN DATEADD(second, -1, DATEADD(day, 1, CONVERT(DATETIME, CONVERT(DATE, @d))))
END
GO
