IF OBJECT_ID('StartMarketplaceUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE StartMarketplaceUpdate AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE StartMarketplaceUpdate
@MarketplaceID INT, 
@UpdatingStart DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @HistoryRecordID INT

	UPDATE MP_CustomerMarketPlace SET
		UpdatingStart = @UpdatingStart,
		UpdatingEnd = NULL
	WHERE
		Id = @MarketplaceID

	INSERT INTO MP_CustomerMarketPlaceUpdatingHistory (
		CustomerMarketPlaceId, UpdatingStart, UpdatingEnd, Error
	) VALUES (
		@MarketplaceID, @UpdatingStart, NULL, NULL
	)

	SET @HistoryRecordID = CONVERT(INT, SCOPE_IDENTITY())
	
	SELECT
		HistoryRecordID   = @HistoryRecordID,
		MarketplaceTypeID = t.InternalId
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
	WHERE
		m.Id = @MarketplaceID
END
GO
