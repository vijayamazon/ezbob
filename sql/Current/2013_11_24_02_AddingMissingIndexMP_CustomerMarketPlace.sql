IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_MarketPlaceId' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_MP_CustomerMarketPlace_MarketPlaceId
	ON [dbo].[MP_CustomerMarketPlace] ([MarketPlaceId])
	INCLUDE ([CustomerId])
END
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CashRequests_IdCustomer' AND object_id = OBJECT_ID('CashRequests'))
BEGIN
CREATE NONCLUSTERED INDEX IX_CashRequests_IdCustomer
ON [dbo].[CashRequests] ([IdCustomer])
END
GO
