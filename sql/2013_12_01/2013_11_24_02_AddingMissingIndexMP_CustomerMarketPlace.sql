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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_EzbobMailNodeAttachRelation_ToField' AND object_id = OBJECT_ID('EzbobMailNodeAttachRelation'))
BEGIN
CREATE NONCLUSTERED INDEX IX_EzbobMailNodeAttachRelation_ToField
ON [dbo].[EzbobMailNodeAttachRelation] ([ToField])
END
GO 

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EbayFeedback_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayFeedback'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_EbayFeedback_CustomerMarketPlaceUpdatingHistoryRecordId
ON [dbo].[MP_EbayFeedback] ([CustomerMarketPlaceUpdatingHistoryRecordId])
INCLUDE ([Id],[CustomerMarketPlaceId],[Created],[RepeatBuyerCount],[RepeatBuyerPercent],[TransactionPercent],[UniqueBuyerCount],[UniqueNegativeCount],[UniquePositiveCount],[UniqueNeutralCount])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EbayUserAccountData_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayUserAccountData'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_EbayUserAccountData_CustomerMarketPlaceUpdatingHistoryRecordId
ON [dbo].[MP_EbayUserAccountData] ([CustomerMarketPlaceUpdatingHistoryRecordId])
INCLUDE ([Id],[CustomerMarketPlaceId],[Created],[PaymentMethod],[PastDue],[CurrentBalance],[CreditCardModifyDate],[CreditCardInfo],[CreditCardExpiration],[BankModifyDate],[AccountState],[AmountPastDueCurrency],[AmountPastDueAmount],[BankAccountInfo],[AccountId],[Currency])
END 
GO
