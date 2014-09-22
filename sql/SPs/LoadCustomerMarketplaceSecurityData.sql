IF OBJECT_ID('LoadCustomerMarketplaceSecurityData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerMarketplaceSecurityData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadCustomerMarketplaceSecurityData
@CustomerID INT,
@DisplayName NVARCHAR(512) = NULL,
@InternalID UNIQUEIDENTIFIER = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		m.Id AS CustomerMarketplaceID,
		m.DisplayName,
		m.UpdatingStart,
		m.SecurityData,
		mt.Name AS MarketplaceType,
		mt.InternalId AS InternalID
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType mt
			ON m.MarketPlaceId = mt.Id
	WHERE
		m.CustomerID = @CustomerID
		AND
		(
			@DisplayName IS NULL
			OR
			m.DisplayName LIKE @DisplayName
		)
		AND
		(
			@InternalID IS NULL
			OR
			mt.InternalId = @InternalID
		)
END
GO
