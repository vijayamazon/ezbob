IF OBJECT_ID('AV_GetCustomerPaymentMarketPlaces') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCustomerPaymentMarketPlaces AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[AV_GetCustomerPaymentMarketPlaces] 
	(@CustomerId INT)
AS
BEGIN
	SET NOCOUNT ON

	SELECT mp.Id Id, mp.DisplayName Name, t.Name Type, mp.OriginationDate
	FROM MP_CustomerMarketPlace mp 
	INNER JOIN MP_MarketplaceType t 
	ON t.Id = mp.MarketPlaceId 
	WHERE CustomerId=@CustomerId 
	AND mp.Disabled = 0 
	AND (
		t.IsPaymentAccount = 1 
		AND t.InternalId<>'3FA5E327-FCFD-483B-BA5A-DC1815747A28' --paypal
		AND t.InternalId<>'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA' --hmrc
		AND t.InternalId<>'107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF' -- yodlee
	)
	
	SET NOCOUNT OFF
END

GO
