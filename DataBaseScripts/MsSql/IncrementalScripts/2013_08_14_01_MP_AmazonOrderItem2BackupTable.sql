IF OBJECT_ID ('dbo.MP_AmazonOrderItem2Backup') IS NOT NULL
	PRINT 'exists'
	--DROP TABLE dbo.MP_AmazonOrderItem2Backup 
GO

CREATE TABLE dbo.MP_AmazonOrderItem2Backup
	(
	  Id                           INT NOT NULL
	, AmazonOrderId                INT
	, OrderId                      NVARCHAR (50)
	, SellerOrderId                NVARCHAR (50)
	, PurchaseDate                 DATETIME
	, LastUpdateDate               DATETIME
	, OrderStatus                  NVARCHAR (50)
	, FulfillmentChannel           NVARCHAR (50)
	, SalesChannel                 NVARCHAR (50)
	, OrderChannel                 NVARCHAR (50)
	, ShipServiceLevel             NVARCHAR (50)
	, OrderTotalCurrency           NVARCHAR (50)
	, OrderTotal                   DECIMAL (18, 8)
	, PaymentMethod                NVARCHAR (50)
	, BuyerName                    NVARCHAR (128)
	, ShipmentServiceLevelCategory NVARCHAR (50)
	, BuyerEmail                   NVARCHAR (128)
	, NumberOfItemsShipped         INT
	, NumberOfItemsUnshipped       INT
	, MarketplaceId                NVARCHAR (50)
	, ShipAddress1                 NVARCHAR (128)
	, ShipAddress2                 NVARCHAR (128)
	, ShipAddress3                 NVARCHAR (128)
	, ShipCity                     NVARCHAR (50)
	, ShipCountryCode              NVARCHAR (50)
	, ShipCounty                   NVARCHAR (50)
	, ShipDistrict                 NVARCHAR (50)
	, ShipName                     NVARCHAR (50)
	, ShipPhone                    NVARCHAR (50)
	, PostalCode                   NVARCHAR (50)
	, StateOrRegion                NVARCHAR (50)
	, CONSTRAINT PK_MP_AmazonOrderItem2Backup PRIMARY KEY (Id)
	)
GO


