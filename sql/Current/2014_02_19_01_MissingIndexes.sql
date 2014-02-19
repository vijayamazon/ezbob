IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_TSP' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TSP
ON [dbo].[LoanTransaction] ([Type],[Status],[PostDate])
INCLUDE ([Amount],[LoanId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_TSP_ALIFLR' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TSP_ALIFLR
ON [dbo].[LoanTransaction] ([Type],[Status],[PostDate])
INCLUDE ([Amount],[LoanId],[Interest],[Fees],[LoanRepayment],[Rollover])
END
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EbayUserAccountData_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_EbayUserAccountData'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_EbayUserAccountData_CustomerMarketPlaceId
ON [dbo].[MP_EbayUserAccountData] ([CustomerMarketPlaceId])
END
GO 

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_TeraPeakCategoryStatistics_OrderItemId' AND object_id = OBJECT_ID('MP_TeraPeakCategoryStatistics'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_TeraPeakCategoryStatistics_OrderItemId
ON [dbo].[MP_TeraPeakCategoryStatistics] ([OrderItemId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EbayUserData_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_EbayUserData'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_EbayUserData_CustomerMarketPlaceId
ON [dbo].[MP_EbayUserData] ([CustomerMarketPlaceId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CashRequests_UC' AND object_id = OBJECT_ID('CashRequests'))
BEGIN
CREATE NONCLUSTERED INDEX IX_CashRequests_UC
ON [dbo].[CashRequests] ([UnderwriterDecision],[CreationDate])
INCLUDE ([Id],[IdCustomer],[UnderwriterDecisionDate],[ManagerApprovedSum],[UnderwriterComment])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_LoanId' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_LoanId
ON [dbo].[LoanTransaction] ([LoanId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CustomerLoyaltyProgram_CustomerMarketPlaceID' AND object_id = OBJECT_ID('CustomerLoyaltyProgram'))
BEGIN
CREATE NONCLUSTERED INDEX IX_CustomerLoyaltyProgram_CustomerMarketPlaceID
ON [dbo].[CustomerLoyaltyProgram] ([CustomerMarketPlaceID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Application_Application_CreatorUserId' AND object_id = OBJECT_ID('Application_Application'))
BEGIN
CREATE NONCLUSTERED INDEX IX_Application_Application_CreatorUserId
ON [dbo].[Application_Application] ([CreatorUserId],[State])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DecisionHistory_CustomerId' AND object_id = OBJECT_ID('DecisionHistory'))
BEGIN
CREATE NONCLUSTERED INDEX IX_DecisionHistory_CustomerId
ON [dbo].[DecisionHistory] ([CustomerId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CustomerAddress_CompanyId' AND object_id = OBJECT_ID('CustomerAddress'))
BEGIN
CREATE NONCLUSTERED INDEX IX_CustomerAddress_CompanyId
ON [dbo].[CustomerAddress] ([CompanyId],[addressType])
END
GO