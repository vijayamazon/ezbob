IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetCustomerMarketPlaces]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetCustomerMarketPlaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_GetCustomerMarketPlaces] 
	(@CustomerId INT)
AS
BEGIN
	SET NOCOUNT ON

	SELECT mp.Id Id, mp.DisplayName Name, t.Name Type, mp.OriginationDate FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_MarketplaceType t 
	ON t.Id = mp.MarketPlaceId 
	WHERE CustomerId=@CustomerId 
	AND mp.Disabled = 0 
	AND (t.IsPaymentAccount = 0 OR t.InternalId='3FA5E327-FCFD-483B-BA5A-DC1815747A28')
	
	SET NOCOUNT OFF
END
GO
