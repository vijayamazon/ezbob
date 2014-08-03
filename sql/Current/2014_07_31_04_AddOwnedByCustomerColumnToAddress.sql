IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'OwnedByCustomer' and Object_ID = Object_ID(N'CustomerAddress'))    
BEGIN
	DECLARE @Statement NVARCHAR(MAX)
	
	SET @Statement = 'ALTER TABLE CustomerAddress ADD OwnedByCustomer BIT'
	
	EXEC(@Statement)
	
	SET @Statement = 'UPDATE CustomerAddress SET OwnedByCustomer = 0'
	
	EXEC(@Statement)
	
	SET @Statement = 'UPDATE 
		CustomerAddress 
	SET 
		OwnedByCustomer = 1 
	FROM 
		Customer, 
		CustomerPropertyStatuses 
	WHERE 
		CustomerAddress.addressType = 1 AND
		Customer.PropertyStatusId = CustomerPropertyStatuses.Id AND
		CustomerPropertyStatuses.IsOwner = 1'
	
	EXEC(@Statement)
END
GO

