IF NOT EXISTS (SELECT * from sys.columns WHERE Name = N'IndustryType' and Object_ID = Object_ID(N'Customer'))
BEGIN 
	ALTER TABLE Customer ADD IndustryType NVARCHAR(50)
END 	
GO


IF NOT EXISTS (SELECT * from sys.columns WHERE Name = N'VatReporting' and Object_ID = Object_ID(N'Company'))
BEGIN 
	ALTER TABLE Company ADD VatReporting NVARCHAR(100)
END 	
GO
