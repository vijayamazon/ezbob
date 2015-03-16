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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EBayOrderItemDetail_PrimaryCategoryId' AND object_id = OBJECT_ID('MP_EBayOrderItemDetail'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_EBayOrderItemDetail_PrimaryCategoryId
ON [dbo].[MP_EBayOrderItemDetail] ([PrimaryCategoryId])
INCLUDE ([Id])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_TeraPeakOrder_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_TeraPeakOrder'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_TeraPeakOrder_CustomerMarketPlaceUpdatingHistoryRecordId
ON [dbo].[MP_TeraPeakOrder] ([CustomerMarketPlaceUpdatingHistoryRecordId])
INCLUDE ([Id],[CustomerMarketPlaceId],[Created],[LastOrderItemEndDate])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Customer_IsTest' AND object_id = OBJECT_ID('Customer'))
BEGIN
CREATE NONCLUSTERED INDEX IX_Customer_IsTest
ON [dbo].[Customer] ([IsTest])
INCLUDE ([Id],[IsOffline])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_TypeStatus' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TypeStatus
ON [dbo].[LoanTransaction] ([Type],[Status])
INCLUDE ([Id],[Amount],[LoanId],[Fees])
END 
GO

