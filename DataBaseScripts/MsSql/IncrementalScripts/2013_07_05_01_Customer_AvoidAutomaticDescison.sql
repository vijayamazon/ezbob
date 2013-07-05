IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'AvoidAutomaticDescison' and Object_ID = Object_ID(N'Customer'))
BEGIN
ALTER TABLE dbo.Customer ADD
	AvoidAutomaticDescison bit NOT NULL CONSTRAINT DF_Customer_AvoidAutomaticDescison DEFAULT 0
END
GO
