IF OBJECT_ID ('dbo.MP_EkmOrder') IS NOT NULL
	DROP TABLE dbo.MP_EkmOrder
GO

CREATE TABLE dbo.MP_EkmOrder
	(
	  Id                                         INT IDENTITY NOT NULL
	, CustomerMarketPlaceId                      INT NOT NULL
	, Created                                    DATETIME NOT NULL
	, CustomerMarketPlaceUpdatingHistoryRecordId INT
	, CONSTRAINT PK_MP_EkmOrder PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_EkmOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)
GO

CREATE INDEX IX_MP_EkmOrderCustomerMarketPlaceId
	ON dbo.MP_EkmOrder (CustomerMarketPlaceId)
GO

