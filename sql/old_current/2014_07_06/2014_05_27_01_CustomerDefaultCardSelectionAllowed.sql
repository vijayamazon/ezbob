IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'DefaultCardSelectionAllowed' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD DefaultCardSelectionAllowed BIT NOT NULL DEFAULT(1)
END 
GO
