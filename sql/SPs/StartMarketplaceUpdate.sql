IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StartMarketplaceUpdate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StartMarketplaceUpdate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StartMarketplaceUpdate] 
	(@MarketplaceId INT, 
	 @UpdatingStart DATETIME)
AS
BEGIN
	UPDATE MP_CustomerMarketPlace SET UpdatingStart = @UpdatingStart WHERE Id = @MarketplaceId
	
	INSERT INTO MP_CustomerMarketPlaceUpdatingHistory
		(CustomerMarketPlaceId, UpdatingStart, UpdatingEnd, Error)
	VALUES
		(@MarketplaceId, @UpdatingStart, NULL, NULL)
	
	SELECT @@IDENTITY AS HistoryRecordId
END
GO
