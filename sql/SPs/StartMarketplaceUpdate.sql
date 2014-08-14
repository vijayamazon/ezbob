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

	SELECT SCOPE_IDENTITY() AS HistoryRecordId
END
GO
