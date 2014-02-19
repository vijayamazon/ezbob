IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_TSP' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TSP
ON [dbo].[LoanTransaction] ([Type],[Status],[PostDate])
INCLUDE ([Amount],[LoanId])
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