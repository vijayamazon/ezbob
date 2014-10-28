
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanScheduleTransaction_ScheduleID' AND object_id = OBJECT_ID('LoanScheduleTransaction'))
BEGIN
	CREATE INDEX IX_LoanScheduleTransaction_ScheduleID ON [ezbob].[dbo].[LoanScheduleTransaction] ([ScheduleID])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LandRegistry_AddressId' AND object_id = OBJECT_ID('LandRegistry'))
BEGIN
	CREATE INDEX IX_LandRegistry_AddressId ON [ezbob].[dbo].[LandRegistry] ([AddressId])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanHistory_LoanIdDate' AND object_id = OBJECT_ID('LoanHistory'))
BEGIN
	CREATE INDEX IX_LoanHistory_LoanIdDate ON [ezbob].[dbo].[LoanHistory] ([LoanId],[Date])
END 
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_EzbobMailNodeAttachRelation_ToField' AND object_id = OBJECT_ID('EzbobMailNodeAttachRelation'))
BEGIN
	CREATE INDEX IX_EzbobMailNodeAttachRelation_ToField ON [ezbob].[dbo].[EzbobMailNodeAttachRelation] ([ToField])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_VatReturnRecords_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_VatReturnRecords'))
BEGIN
	CREATE INDEX IX_MP_VatReturnRecords_CustomerMarketPlaceId ON [ezbob].[dbo].[MP_VatReturnRecords] ([CustomerMarketPlaceId])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_ChannelGrabberOrderItem_OrderIdPaymentDate' AND object_id = OBJECT_ID('MP_ChannelGrabberOrderItem'))
BEGIN
	CREATE INDEX IX_MP_ChannelGrabberOrderItem_OrderIdPaymentDate ON [ezbob].[dbo].[MP_ChannelGrabberOrderItem] ([OrderId],[PaymentDate])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_VatReturnSummary_CustomerMarketplaceID' AND object_id = OBJECT_ID('MP_VatReturnSummary'))
BEGIN
	CREATE INDEX IX_MP_VatReturnSummary_CustomerMarketplaceID ON [ezbob].[dbo].[MP_VatReturnSummary] ([CustomerMarketplaceID])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_PayPalTransaction_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_PayPalTransaction'))
BEGIN
	CREATE INDEX IX_MP_PayPalTransaction_CustomerMarketPlaceId ON [ezbob].[dbo].[MP_PayPalTransaction] ([CustomerMarketPlaceId])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_ExperianHistory_TypeDirectorId' AND object_id = OBJECT_ID('MP_ExperianHistory'))
BEGIN
	CREATE INDEX IX_MP_ExperianHistory_TypeDirectorId ON [ezbob].[dbo].[MP_ExperianHistory] ([Type], [DirectorId])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_YodleeAccounts_CustomerId' AND object_id = OBJECT_ID('YodleeAccounts'))
BEGIN
	CREATE INDEX IX_YodleeAccounts_CustomerId ON [ezbob].[dbo].[YodleeAccounts] ([CustomerId])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CompanyEmployeeCount_CustomerId' AND object_id = OBJECT_ID('CompanyEmployeeCount'))
BEGIN
	CREATE INDEX IX_CompanyEmployeeCount_CustomerId ON [ezbob].[dbo].[CompanyEmployeeCount] ([CustomerId])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_EmailConfirmationRequest_CustomerIdId' AND object_id = OBJECT_ID('EmailConfirmationRequest'))
BEGIN
	CREATE INDEX IX_EmailConfirmationRequest_CustomerIdId ON [ezbob].[dbo].[EmailConfirmationRequest] ([CustomerId],[Id])
END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MobileCodes_PhoneCodeActive' AND object_id = OBJECT_ID('MobileCodes'))
BEGIN
	CREATE INDEX IX_MobileCodes_PhoneCodeActive ON [ezbob].[dbo].[MobileCodes] ([Phone], [Code], [Active])
END 
GO

