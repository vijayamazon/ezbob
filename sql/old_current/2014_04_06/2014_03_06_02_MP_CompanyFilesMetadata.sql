IF OBJECT_ID (N'dbo.MP_CompanyFilesMetaData') IS NULL
BEGIN 
CREATE TABLE MP_CompanyFilesMetaData(
	  Id INT NOT NULL IDENTITY
	, CustomerId INT NOT NULL
	, Created DATETIME
	, FileName NVARCHAR(300)
	, FilePath NVARCHAR(MAX)
	, CONSTRAINT PK_MP_CompanyFilesMetaData PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_CompanyFilesMetaData_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
)

CREATE INDEX IX_MP_CompanyFilesMetaData_CustomerMarketPlaceId ON dbo.MP_CompanyFilesMetaData (CustomerId)
END 		
GO

