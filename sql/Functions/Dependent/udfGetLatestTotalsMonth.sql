IF OBJECT_ID('dbo.udfGetLatestTotalsMonth') IS NOT NULL
	DROP FUNCTION dbo.udfGetLatestTotalsMonth
GO

SET QUOTED_IDENTIFIER ON
GO

/*

Returns a month start from which totals calculation should be started.

What: if current date (@Now) and the last update date of the marketplace (@MpID) are in
the tail of the same month then start calculating total from the current month, otherwise
start with the previous month.

How:

if current date is null
	result is null
else if tail length is not defined properly
	result is previuos month
else if current date is not in the tail of the month
	result is previuos month
else if last update time and current date are not in the same month
	result is previuos month
else if last update time is not in the tail of the month
	result is previuos month
else
	result is current month

*/

CREATE FUNCTION dbo.udfGetLatestTotalsMonth(
@MpID INT,
@Now DATETIME
)
RETURNS DATETIME
AS
BEGIN
	IF @Now IS NULL
		RETURN NULL

	DECLARE @TailLen INT = CONVERT(INT,
		(SELECT TOP 1 Value FROM ConfigurationVariables WHERE Name = 'TotalsMonthTail')
	)

	IF @TailLen <= 0
		RETURN dbo.udfMonthStart(DATEADD(month, -1, @Now))

	DECLARE @TailStart DATETIME =
		DATEADD(day, -@TailLen, DATEADD(month, 1, dbo.udfMonthStart(@Now)))

	IF @Now < @TailStart
		RETURN dbo.udfMonthStart(DATEADD(month, -1, @Now))

	DECLARE @HistoryID INT = (
		SELECT TOP 1
			h.Id
		FROM
			MP_CustomerMarketPlaceUpdatingHistory h
		WHERE
			h.CustomerMarketPlaceId = @MpID
			AND
			h.UpdatingEnd <= @Now
			AND (
				h.Error IS NULL
				OR
				LTRIM(RTRIM(h.Error)) = ''
			)
		ORDER BY
			h.UpdatingEnd DESC,
			h.Id DESC
	);

	IF @HistoryID IS NULL
		RETURN dbo.udfMonthStart(DATEADD(month, -1, @Now))

	DECLARE @d DATETIME = (
		SELECT
			h.UpdatingEnd
		FROM
			MP_CustomerMarketPlaceUpdatingHistory h
		WHERE
			h.Id = @HistoryID
	)

	IF DATEPART(month, @d) != DATEPART(month, @Now) OR DATEPART(year, @d) != DATEPART(year, @Now)
		RETURN dbo.udfMonthStart(DATEADD(month, -1, @Now))

	IF @d < @TailStart
		RETURN dbo.udfMonthStart(DATEADD(month, -1, @Now))

	RETURN dbo.udfMonthStart(@Now)
END
GO
