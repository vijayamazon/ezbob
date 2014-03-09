IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'FileContentType' AND Object_ID = Object_ID(N'MP_CompanyFilesMetaData'))
	ALTER TABLE MP_CompanyFilesMetaData ADD FileContentType NVARCHAR(300)
	
GO 	