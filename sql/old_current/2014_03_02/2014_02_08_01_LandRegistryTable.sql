IF OBJECT_ID (N'dbo.LandRegistry') IS NULL
BEGIN 
CREATE TABLE LandRegistry(
	  Id INT NOT NULL IDENTITY(1,1)
	, CustomerId INT NOT NULL
	, InsertDate   DATETIME
	, Postcode NVARCHAR(15)
	, TitleNumber NVARCHAR(30)
	, RequestType NVARCHAR(20)
	, ResponseType NVARCHAR(20)
	, Request NVARCHAR(MAX)
	, Response NVARCHAR(MAX)
	, AttachmentPath NVARCHAR(300)
	, CONSTRAINT PK_LandRegistry PRIMARY KEY (Id)
	, CONSTRAINT FK_LandRegistry_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
)
END 	
GO

