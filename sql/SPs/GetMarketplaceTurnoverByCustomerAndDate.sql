SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetMarketplaceTurnoverByCustomerAndDate') IS NULL
	EXECUTE('CREATE PROCEDURE GetMarketplaceTurnoverByCustomerAndDate AS SELECT 1')
GO

ALTER PROCEDURE GetMarketplaceTurnoverByCustomerAndDate
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		t.AggID,
		t.TheMonth,
		t.IsActive,
		t.CustomerMarketPlaceUpdatingHistoryID,
		t.Turnover,
		t.CustomerMarketPlaceId,
		t.UpdatingEnd,
		t.CustomerId,
		t.IsMarketplaceDisabled,
		t.MarketplaceInternalID,
		t.IsPaymentAccount
	FROM
		MarketplaceTurnover t
	WHERE
		t.CustomerID = @CustomerID
		AND
		t.UpdatingEnd < @Now
		AND
		t.IsMarketplaceDisabled = 0
END
GO
