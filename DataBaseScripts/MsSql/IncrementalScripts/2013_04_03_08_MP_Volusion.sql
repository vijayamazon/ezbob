IF OBJECT_ID('dbo.MP_VolusionOrder') IS NULL
BEGIN
	CREATE TABLE dbo.MP_VolusionOrder (
		Id                                         INT IDENTITY NOT NULL
	,	CustomerMarketPlaceId                      INT NOT NULL
	,	Created                                    DATETIME NOT NULL
	,	CustomerMarketPlaceUpdatingHistoryRecordId INT
	,	CONSTRAINT PK_MP_VolusionOrder PRIMARY KEY (Id)
	,	CONSTRAINT FK_MP_VolusionOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)

	CREATE INDEX IX_MP_VolusionOrderCustomerMarketPlaceId ON dbo.MP_VolusionOrder (CustomerMarketPlaceId)
END
GO

IF OBJECT_ID('dbo.MP_VolusionOrderItem') IS NULL
BEGIN
	CREATE TABLE dbo.MP_VolusionOrderItem (
		Id                 INT IDENTITY NOT NULL
	,	OrderId            INT
	,	NativeOrderId      NVARCHAR(300)
	,	TotalCost          NUMERIC(18, 2)
	,	CurrencyCode       NVARCHAR(3)
	,	PaymentDate        DATETIME
	,	PurchaseDate       DATETIME
	,	OrderStatus        NVARCHAR(300)
	,	CONSTRAINT PK_MP_VolusionOrderItem PRIMARY KEY (Id)
	,	CONSTRAINT FK_MP_VolusionOrderItem_MP_Order FOREIGN KEY (OrderId) REFERENCES dbo.MP_VolusionOrder (Id)
	)

	CREATE INDEX IX_MP_VolusionOrderItemOrderId ON dbo.MP_VolusionOrderItem (OrderId)
END
GO

IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId = 'afca0e18-05e3-400f-8af4-b1bcae09375c')
BEGIN
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description])
		VALUES ('Volusion', 'afca0e18-05e3-400f-8af4-b1bcae09375c', 'Volusion')
END
GO
