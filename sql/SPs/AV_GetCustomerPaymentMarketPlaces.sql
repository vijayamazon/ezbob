IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetCustomerPaymentMarketPlaces]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetCustomerPaymentMarketPlaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_GetCustomerPaymentMarketPlaces] 
	(@CustomerId INT)
AS
BEGIN
	SET NOCOUNT ON

	SELECT t.Name Type
	FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_MarketplaceType t 
	ON t.Id = mp.MarketPlaceId 
	WHERE CustomerId=@CustomerId 
	AND mp.Disabled = 0 
	AND (t.IsPaymentAccount = 1 AND t.InternalId<>'3FA5E327-FCFD-483B-BA5A-DC1815747A28')
	
	SET NOCOUNT OFF
END
GO
