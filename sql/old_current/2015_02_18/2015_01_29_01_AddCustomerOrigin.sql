IF object_id('CustomerOrigin') IS NULL
BEGIN
	CREATE TABLE CustomerOrigin(  
		CustomerOriginID INT NOT NULL IDENTITY(1,1),
		Name NVARCHAR(20),
		CONSTRAINT PK_CustomerOrigin PRIMARY KEY (CustomerOriginID)
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM CustomerOrigin)
BEGIN
	INSERT INTO CustomerOrigin (Name) VALUES ('ezbob')
	INSERT INTO CustomerOrigin (Name) VALUES ('everline')
END
GO

IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE id=object_id('Customer') AND name='OriginID')
BEGIN
	ALTER TABLE Customer ADD OriginID INT
	ALTER TABLE Customer ADD CONSTRAINT FK_Customer_CustomerOrigin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID)
	
END
GO

IF NOT EXISTS (SELECT 1 FROM Customer WHERE OriginID IS NOT NULL)
BEGIN
	UPDATE Customer SET OriginID = (SELECT CustomerOriginID FROM CustomerOrigin WHERE Name = 'ezbob')
END
GO
