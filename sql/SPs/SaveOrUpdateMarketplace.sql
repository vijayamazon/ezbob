SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveOrUpdateMarketplace') IS NULL
	EXECUTE('CREATE PROCEDURE SaveOrUpdateMarketplace AS SELECT 1')
GO

ALTER PROCEDURE SaveOrUpdateMarketplace
@CustomerId INT,
@SecurityData VARBINARY(MAX),
@MarketPlaceUniqueID UNIQUEIDENTIFIER,
@DisplayName NVARCHAR(512),
@AmazonMarketPlaceTypeId NVARCHAR(20),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #input (
		MpID INT NULL,
		CustomerID INT NULL,
		SecurityData VARBINARY(MAX) NULL,
		MpTypeID INT NULL,
		MpUniqueID UNIQUEIDENTIFIER NULL,
		DisplayName NVARCHAR(512) NULL,
		AmazonMarketplaceID INT NULL,
		AmazonMarketplaceTypeID NVARCHAR(20) NULL,
		Now DATETIME NULL
	)

	INSERT INTO #input(MpID, CustomerID, SecurityData, MpTypeID, MPUniqueID, DisplayName, AmazonMarketplaceID, AmazonMarketplaceTypeID)
	SELECT TOP 1
		m.Id,
		@CustomerID,
		@SecurityData,
		t.Id,
		@MarketPlaceUniqueID,
		@DisplayName,
		a.Id,
		a.MarketplaceId
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON t.InternalId = @MarketPlaceUniqueID
		LEFT JOIN MP_AmazonMarketplaceType a ON a.MarketplaceId = @AmazonMarketPlaceTypeId
	WHERE
		t.InternalId = @MarketPlaceUniqueID
		AND (
			(m.CustomerId = @CustomerID AND m.DisplayName = @DisplayName)
		)

	select * From #input

	DROP TABLE #input
END
GO
