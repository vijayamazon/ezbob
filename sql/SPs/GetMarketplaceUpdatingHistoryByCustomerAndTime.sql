SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetMarketplaceUpdatingHistoryByCustomerAndTime') IS NULL
	EXECUTE('CREATE PROCEDURE GetMarketplaceUpdatingHistoryByCustomerAndTime AS SELECT 1')
GO

ALTER PROCEDURE GetMarketplaceUpdatingHistoryByCustomerAndTime
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		h.Id
	FROM
		MP_CustomerMarketPlaceUpdatingHistory h
		INNER JOIN MP_CustomerMarketPlace m ON h.CustomerMarketPlaceId = m.Id
	WHERE
		m.CustomerId = @CustomerID
		AND
		h.UpdatingEnd < @Now
		AND
		(h.Error IS NULL OR LTRIM(RTRIM(h.Error)) = '')
END
GO
