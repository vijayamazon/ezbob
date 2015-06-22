IF OBJECT_ID('dbo.udfGetLatestTotalsMonth') IS NOT NULL
	DROP FUNCTION dbo.udfGetLatestTotalsMonth
GO

SET QUOTED_IDENTIFIER ON
GO

/*

Returns a month start from which totals calculation should be started

Special handling for HMRC marketplace.

What:
1. detect current month using standard procedure;
2. check what is the last existing month (i.e. having data) for the marketplace that is less
   than or equal to the current month;
3. if the last existing month is no more than 3 full months away from the current month
   then return value is the last existing month;
4. otherwise return value is current month - 4 (i.e. if today is June returned value is
   February).

*/

CREATE FUNCTION dbo.udfGetLatestTotalsMonth(
@MpID INT,
@Now DATETIME
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @CurrentMonth DATETIME = dbo.udfGetLatestTotalsMonth(@MpID, @Now)

	IF @CurrentMonth IS NULL
		RETURN NULL

	DECLARE @HMRC UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	IF NOT EXISTS (
		SELECT
			m.Id
		FROM
			MP_CustomerMarketPlace m
			INNER JOIN MP_MarketplaceType t
				ON m.MarketPlaceId = t.Id
				AND t.InternalId = @HMRC
		WHERE
			m.Id = @MpID
	)
	BEGIN
		RETURN @CurrentMonth
	END

	DECLARE @LastExistingMonth DATETIME = (
		SELECT
			MAX(t.TheMonth)
		FROM
			HmrcAggregationTurnover t
		WHERE
			t.CustomerMarketPlaceId = @MpID
			AND
			t.TheMonth <= @CurrentMonth
	)

	IF @LastExistingMonth IS NULL
		RETURN dbo.udfMonthStart(DATEADD(month, -4, @CurrentMonth))

	IF DATEDIFF(month, @LastExistingMonth, @CurrentMonth) <= 4
		RETURN dbo.udfMonthStart(@LastExistingMonth)

	RETURN dbo.udfMonthStart(DATEADD(month, -4, @CurrentMonth))
END
GO
