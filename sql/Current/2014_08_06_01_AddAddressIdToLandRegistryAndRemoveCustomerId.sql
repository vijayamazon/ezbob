IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AddressId' and Object_ID = Object_ID(N'LandRegistry'))    
BEGIN
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'ALTER TABLE LandRegistry ADD AddressId INT'
	
	EXEC(@Statement)
	
	SET @Statement = 'UPDATE LandRegistry SET LandRegistry.AddressId = CustomerAddress.addressId FROM CustomerAddress WHERE LandRegistry.CustomerId = CustomerAddress.CustomerId AND CustomerAddress.addressType = 1'
	
	EXEC(@Statement)
END
GO

