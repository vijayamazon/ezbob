IF OBJECT_ID('LoadCustomerMarketplaceSecurityData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerMarketplaceSecurityData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadCustomerMarketplaceSecurityData
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		m.Id AS ID,
		m.DisplayName,
		m.SecurityData,
		mt.Name AS MarketplaceType,
		mt.InternalId
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType mt
			ON m.MarketPlaceId = mt.Id
	WHERE
		m.CustomerID = @CustomerID
END
GO
