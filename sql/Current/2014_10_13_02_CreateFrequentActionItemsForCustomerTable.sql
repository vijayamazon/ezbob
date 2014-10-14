IF object_id('FrequentActionItemsForCustomer') IS NOT NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MarkedDate' and Object_ID = Object_ID(N'FrequentActionItemsForCustomer'))    
	BEGIN
		DROP TABLE FrequentActionItemsForCustomer
	END
END
GO

IF object_id('FrequentActionItemsForCustomer') IS NULL
BEGIN
	CREATE TABLE FrequentActionItemsForCustomer(
	    Id INT NOT NULL IDENTITY(1,1)
	   ,CustomerId INT NOT NULL
	   ,ItemId INT NOT NULL
	   ,MarkedDate DATETIME
	   ,UnmarkedDate DATETIME
	   ,CONSTRAINT PK_FrequentActionItemsForCustomer PRIMARY KEY (Id)
	)	
END
GO
