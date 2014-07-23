IF OBJECT_ID('GetCustomerZooplaData') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerZooplaData AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerZooplaData
@CustomerId INT
AS
BEGIN
	SELECT 
		AverageSoldPrice1Year 
	FROM 
		Zoopla 
	WHERE 
		CustomerAddressId IN 
		(
			SELECT 
				addressId 
			FROM 
				CustomerAddress 
			WHERE 
				CustomerId = @CustomerId
		)
END
GO
