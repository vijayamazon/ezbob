IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsAlibaba' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD IsAlibaba BIT NOT NULL DEFAULT(0)
END
GO