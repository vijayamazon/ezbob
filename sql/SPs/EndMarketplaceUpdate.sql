IF OBJECT_ID('EndMarketplaceUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE EndMarketplaceUpdate AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EndMarketplaceUpdate
@MarketplaceID INT,
@HistoryRecordID INT,
@UpdatingEnd DATETIME,
@ErrorMessage NVARCHAR(MAX),
@TokenExpired INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE MP_CustomerMarketPlace SET
		UpdatingEnd = @UpdatingEnd,
		UpdateError = @ErrorMessage,
		TokenExpired = @TokenExpired
	WHERE
		Id = @MarketplaceID

	UPDATE MP_CustomerMarketPlaceUpdatingHistory SET
		UpdatingEnd = @UpdatingEnd,
		Error = @ErrorMessage
	WHERE
		Id = @HistoryRecordID
END
GO
