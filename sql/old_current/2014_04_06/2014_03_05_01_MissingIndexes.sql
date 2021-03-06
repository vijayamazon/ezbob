IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_EzServiceActionHistory_ActionID' AND object_id = OBJECT_ID('EzServiceActionHistory'))
BEGIN
CREATE NONCLUSTERED INDEX IX_EzServiceActionHistory_ActionID ON [ezbob].[dbo].[EzServiceActionHistory] ([ActionID])
CREATE NONCLUSTERED INDEX IX_CustomerRelations_CustomerId ON [ezbob].[dbo].[CustomerRelations] ([CustomerId])
CREATE NONCLUSTERED INDEX IX_CustomerLoyaltyProgram_CustomerID ON [ezbob].[dbo].[CustomerLoyaltyProgram] ([CustomerID])
CREATE NONCLUSTERED INDEX IX_EzServiceActionHistory_ActionNameID_CustomerID ON [ezbob].[dbo].[EzServiceActionHistory] ([ActionNameID], [CustomerID])
CREATE NONCLUSTERED INDEX IX_MP_TeraPeakOrderItem_TeraPeakOrderId ON [ezbob].[dbo].[MP_TeraPeakOrderItem] ([TeraPeakOrderId])
CREATE NONCLUSTERED INDEX IX_CustomerInviteFriend_InvitedByFriendSource_CustomerId ON [ezbob].[dbo].[CustomerInviteFriend] ([InvitedByFriendSource],[CustomerId])
END 
GO

