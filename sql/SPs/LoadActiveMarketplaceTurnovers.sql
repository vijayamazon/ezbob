SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadActiveMarketplaceTurnovers') IS NULL
	EXECUTE('CREATE PROCEDURE LoadActiveMarketplaceTurnovers AS SELECT 1')
GO

ALTER PROCEDURE LoadActiveMarketplaceTurnovers
@MpID INT
AS
BEGIN
	SELECT
		a.AggID,
		a.TheMonth,
		a.IsActive,
		a.CustomerMarketPlaceUpdatingHistoryID,
		a.Turnover,
		a.CustomerMarketPlaceId,
		a.UpdatingEnd,
		a.CustomerId,
		a.IsMarketplaceDisabled,
		a.MarketplaceInternalID,
		a.IsPaymentAccount
	FROM
		MarketplaceTurnover a
	WHERE
		a.CustomerMarketplaceId = @MpID
		AND
		a.IsActive = 1
	ORDER BY
		a.TheMonth DESC
END
GO
