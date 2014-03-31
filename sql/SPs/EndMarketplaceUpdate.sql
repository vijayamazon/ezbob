IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EndMarketplaceUpdate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[EndMarketplaceUpdate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EndMarketplaceUpdate] 
	(@MarketplaceId INT,
	 @HistoryRecordId INT,
	 @UpdatingEnd DATETIME,
	 @ErrorMessage NVARCHAR(MAX),
	 @TokenExpired INT)
AS
BEGIN
	UPDATE MP_CustomerMarketPlace SET UpdatingEnd = @UpdatingEnd, UpdateError = @ErrorMessage, TokenExpired = @TokenExpired WHERE Id = @MarketplaceId
		
	UPDATE MP_CustomerMarketPlaceUpdatingHistory SET UpdatingEnd = @UpdatingEnd, Error = @ErrorMessage WHERE Id = @HistoryRecordId
END
GO
