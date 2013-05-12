IF OBJECT_ID ('dbo.MP_YodleeOrder') IS NOT NULL
	DROP TABLE dbo.MP_YodleeOrder
GO

CREATE TABLE dbo.MP_YodleeOrder
	(
	  Id                                         INT IDENTITY NOT NULL
	, CustomerMarketPlaceId                      INT NOT NULL
	, Created                                    DATETIME NOT NULL
	, CustomerMarketPlaceUpdatingHistoryRecordId INT
	, CONSTRAINT PK_MP_YodleeOrder PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_YodleeOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)
GO

CREATE INDEX IX_MP_YodleeOrderCustomerMarketPlaceId
	ON dbo.MP_YodleeOrder (CustomerMarketPlaceId)
GO

