IF OBJECT_ID('dbo.MP_PlayOrder') IS NULL
BEGIN
	CREATE TABLE dbo.MP_PlayOrder (
		Id                                         INT IDENTITY NOT NULL
	,	CustomerMarketPlaceId                      INT NOT NULL
	,	Created                                    DATETIME NOT NULL
	,	CustomerMarketPlaceUpdatingHistoryRecordId INT
	,	CONSTRAINT PK_MP_PlayOrder PRIMARY KEY (Id)
	,	CONSTRAINT FK_MP_PlayOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)

	CREATE INDEX IX_MP_PlayOrderCustomerMarketPlaceId ON dbo.MP_PlayOrder (CustomerMarketPlaceId)
END
GO

IF OBJECT_ID('dbo.MP_PlayOrderItem') IS NULL
BEGIN
	CREATE TABLE dbo.MP_PlayOrderItem (
		Id                 INT IDENTITY NOT NULL
	,	OrderId            INT
	,	NativeOrderId      NVARCHAR(300)
	,	TotalCost          NUMERIC(18, 2)
	,	CurrencyCode       NVARCHAR(3)
	,	PaymentDate        DATETIME
	,	PurchaseDate       DATETIME
	,	OrderStatus        NVARCHAR(300)
	,	CONSTRAINT PK_MP_PlayOrderItem PRIMARY KEY (Id)
	,	CONSTRAINT FK_MP_PlayOrderItem_MP_Order FOREIGN KEY (OrderId) REFERENCES dbo.MP_PlayOrder (Id)
	)

	CREATE INDEX IX_MP_PlayOrderItemOrderId ON dbo.MP_PlayOrderItem (OrderId)
END
GO

IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId = 'a5e96d38-fd2e-4e54-9e0c-276493c950a6')
BEGIN
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description])
		VALUES ('Play', 'a5e96d38-fd2e-4e54-9e0c-276493c950a6', 'Play.com')
END
GO
