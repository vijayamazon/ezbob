IF OBJECT_ID('dbo.MP_ChannelGrabberOrder') IS NULL
BEGIN
	CREATE TABLE dbo.MP_ChannelGrabberOrder (
		Id                                         INT IDENTITY NOT NULL
	,	CustomerMarketPlaceId                      INT NOT NULL
	,	Created                                    DATETIME NOT NULL
	,	CustomerMarketPlaceUpdatingHistoryRecordId INT
	,	CONSTRAINT PK_MP_ChannelGrabberOrder PRIMARY KEY (Id)
	,	CONSTRAINT FK_MP_ChannelGrabberOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)

	CREATE INDEX IX_MP_ChannelGrabberOrderCustomerMarketPlaceId ON dbo.MP_ChannelGrabberOrder (CustomerMarketPlaceId)
END
GO

IF OBJECT_ID('dbo.MP_ChannelGrabberOrderItem') IS NULL
BEGIN
	CREATE TABLE dbo.MP_ChannelGrabberOrderItem (
		Id                 INT IDENTITY NOT NULL
	,	OrderId            INT
	,	NativeOrderId      NVARCHAR(300)
	,	TotalCost          NUMERIC(18, 2)
	,	CurrencyCode       NVARCHAR(3)
	,	PaymentDate        DATETIME
	,	PurchaseDate       DATETIME
	,	OrderStatus        NVARCHAR(300)
	,	CONSTRAINT PK_MP_ChannelGrabberOrderItem PRIMARY KEY (Id)
	,	CONSTRAINT FK_MP_ChannelGrabberOrderItem_MP_Order FOREIGN KEY (OrderId) REFERENCES dbo.MP_ChannelGrabberOrder (Id)
	)

	CREATE INDEX IX_MP_ChannelGrabberOrderItemOrderId ON dbo.MP_ChannelGrabberOrderItem (OrderId)
END
GO
