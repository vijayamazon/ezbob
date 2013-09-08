IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsAddCollectionFee' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer DROP COLUMN IsAddCollectionFee
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CollectionFee' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer DROP COLUMN CollectionFee
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CollectionDateOfDeclaration' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer DROP COLUMN CollectionDateOfDeclaration
END
GO

