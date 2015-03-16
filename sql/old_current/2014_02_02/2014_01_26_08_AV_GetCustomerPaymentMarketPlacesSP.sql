IF OBJECT_ID('AV_GetCustomerPaymentMarketPlaces') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCustomerPaymentMarketPlaces AS SELECT 1')
GO

ALTER PROCEDURE AV_GetCustomerPaymentMarketPlaces
@CustomerId INT
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

