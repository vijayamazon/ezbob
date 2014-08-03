IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'OwnedByCustomer' and Object_ID = Object_ID(N'CustomerAddress'))    
BEGIN
	ALTER TABLE CustomerAddress ADD OwnedByCustomer BIT
	
	UPDATE CustomerAddress SET OwnedByCustomer = 0
	
	UPDATE 
		CustomerAddress 
	SET 
		OwnedByCustomer = 1 
	FROM 
		Customer, 
		CustomerPropertyStatuses 
	WHERE 
		CustomerAddress.addressType = 1 AND
		Customer.PropertyStatusId = CustomerPropertyStatuses.Id AND
		CustomerPropertyStatuses.IsOwner = 1
END
GO

