IF object_id('FrequentActionItemsForCustomer') IS NULL
BEGIN
	CREATE TABLE FrequentActionItemsForCustomer(
	    Id INT NOT NULL IDENTITY(1,1)
	   ,CustomerId INT NOT NULL
	   ,ItemId INT NOT NULL
	   ,CONSTRAINT PK_FrequentActionItemsForCustomer PRIMARY KEY (Id)
	)	
END
GO
