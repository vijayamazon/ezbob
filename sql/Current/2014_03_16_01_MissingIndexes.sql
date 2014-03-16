IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EkmOrderItem_OrderIdOrderDate' AND object_id = OBJECT_ID('MP_EkmOrderItem'))
BEGIN
CREATE INDEX IX_MP_EkmOrderItem_OrderIdOrderDate ON [ezbob].[dbo].[MP_EkmOrderItem] ([OrderId],[OrderDate])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_TeraPeakOrder_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_TeraPeakOrder'))
BEGIN
CREATE INDEX IX_MP_TeraPeakOrder_CustomerMarketPlaceId ON [ezbob].[dbo].[MP_TeraPeakOrder] ([CustomerMarketPlaceId])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_ExperianBankCache_KeyData' AND object_id = OBJECT_ID('MP_ExperianBankCache'))
BEGIN
CREATE INDEX IX_MP_ExperianBankCache_KeyData ON [ezbob].[dbo].[MP_ExperianBankCache] ([KeyData])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_FraudDetection_FraudRequestId' AND object_id = OBJECT_ID('FraudDetection'))
BEGIN
CREATE INDEX IX_FraudDetection_FraudRequestId ON [ezbob].[dbo].[FraudDetection] ([FraudRequestId])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_PayPointCard_CustomerId' AND object_id = OBJECT_ID('PayPointCard'))
BEGIN
CREATE INDEX IX_PayPointCard_CustomerId ON [ezbob].[dbo].[PayPointCard] ([CustomerId])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_PayPalTransaction_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_PayPalTransaction'))
BEGIN
CREATE INDEX IX_MP_PayPalTransaction_CustomerMarketPlaceUpdatingHistoryRecordId ON [ezbob].[dbo].[MP_PayPalTransaction] ([CustomerMarketPlaceUpdatingHistoryRecordId])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_AmazonFeedback_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_AmazonFeedback'))
BEGIN
CREATE INDEX IX_MP_AmazonFeedback_CustomerMarketPlaceUpdatingHistoryRecordId ON [ezbob].[dbo].[MP_AmazonFeedback] ([CustomerMarketPlaceUpdatingHistoryRecordId])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_AmazonOrder_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_AmazonOrder'))
BEGIN
CREATE INDEX IX_MP_AmazonOrder_CustomerMarketPlaceUpdatingHistoryRecordId ON [ezbob].[dbo].[MP_AmazonOrder] ([CustomerMarketPlaceUpdatingHistoryRecordId])
END 
GO
