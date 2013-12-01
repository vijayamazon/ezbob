IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_PayPalTransactionItem2_TransactionId' AND object_id = OBJECT_ID('MP_PayPalTransactionItem2'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_MP_PayPalTransactionItem2_TransactionId
	ON [dbo].[MP_PayPalTransactionItem2] ([TransactionId])
	INCLUDE ([Id],[Created],[CurrencyId],[FeeAmount],[GrossAmount],[NetAmount],[TimeZone],[Type],[Status],[PayPalTransactionId])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlaceUpdatingHistory_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_CustomerMarketPlaceUpdatingHistory'))
BEGIN
	CREATE NONCLUSTERED INDEX IX_MP_CustomerMarketPlaceUpdatingHistory_CustomerMarketPlaceId
	ON [dbo].[MP_CustomerMarketPlaceUpdatingHistory] ([CustomerMarketPlaceId])
END 
GO