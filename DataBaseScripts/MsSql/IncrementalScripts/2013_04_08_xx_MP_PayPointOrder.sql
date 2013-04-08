IF OBJECT_ID ('dbo.MP_PayPointOrder') IS NOT NULL
BEGIN
	PRINT 'MP_PayPointOrder exists'	
	DROP TABLE MP_PayPointOrder
END
ELSE
BEGIN
CREATE TABLE dbo.MP_PayPointOrder
	(
	  Id                                         INT IDENTITY NOT NULL
	, CustomerMarketPlaceId                      INT NOT NULL
	, Created                                    DATETIME NOT NULL
	, CustomerMarketPlaceUpdatingHistoryRecordId INT
	, CONSTRAINT PK_MP_PayPointOrder PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_PayPointOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)
GO
END

CREATE INDEX IX_MP_EkmOrderCustomerMarketPlaceId
	ON dbo.MP_PayPointOrder (CustomerMarketPlaceId)
GO

