IF OBJECT_ID('GetCustomerMpsErrors') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerMpsErrors AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerMpsErrors
@CustomerId INT
AS
BEGIN
	SELECT 
		Name, 
		UpdateError 
	FROM 
		MP_CustomerMarketPlace, 
		MP_MarketplaceType 
	WHERE 
		CustomerId = @CustomerId AND 
		MarketPlaceId = MP_MarketplaceType.Id
END
GO
