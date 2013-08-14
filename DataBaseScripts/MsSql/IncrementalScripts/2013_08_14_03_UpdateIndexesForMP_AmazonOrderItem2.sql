DROP INDEX dbo.MP_AmazonOrderItem2.IX_AMO2_MarketPlaceID
GO

DROP INDEX dbo.MP_AmazonOrderItem2.IX_AMO2_MarketPlaceId_OS
GO

DROP INDEX dbo.MP_AmazonOrderItem2.IX_AMO2_MarketPlaceId_PD
GO

DROP INDEX dbo.MP_AmazonOrderItem2.IX_AMO2_MarketPlaceId_PD2
GO

DROP INDEX dbo.MP_AmazonOrderItem2.IX_MP_AmazonOrderItem2_AOId
GO

CREATE INDEX IX_AMO2_MarketPlaceId_OS
	ON dbo.MP_AmazonOrderItem2 (OrderStatus, Id, AmazonOrderId, PurchaseDate)
GO

CREATE INDEX IX_AMO2_MarketPlaceId_PD
	ON dbo.MP_AmazonOrderItem2 (AmazonOrderId, PurchaseDate)
GO

CREATE INDEX IX_AMO2_MarketPlaceId_PD2
	ON dbo.MP_AmazonOrderItem2 (AmazonOrderId, PurchaseDate)
GO

CREATE INDEX IX_MP_AmazonOrderItem2_AOId
	ON dbo.MP_AmazonOrderItem2 (AmazonOrderId, Id, OrderId, SellerOrderId, PurchaseDate, LastUpdateDate, OrderStatus, OrderTotalCurrency, OrderTotal, NumberOfItemsShipped, NumberOfItemsUnshipped)
GO
