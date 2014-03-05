IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EKMGetShopByCustomerId]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[EKMGetShopByCustomerId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EKMGetShopByCustomerId] 
	(@CustomerId INT)
AS
BEGIN
	SELECT * FROM MP_CustomerMarketPlace WHERE MarketPlaceId = 4 AND CustomerId=@CustomerId
END
GO
