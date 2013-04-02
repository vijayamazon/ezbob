/****** Object:  Index [IX_MP_PayPalTransactionItem_Payer]    Script Date: 01/21/2013 15:48:01 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem]') 
AND name = N'IX_MP_PayPalTransactionItem_Payer')
DROP INDEX [IX_MP_PayPalTransactionItem_Payer] ON [dbo].[MP_PayPalTransactionItem] WITH ( ONLINE = OFF )
GO


/****** Object:  Index [IX_MP_PayPalTransactionItem_Payer]    Script Date: 01/21/2013 15:48:01 ******/
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem_Payer] ON [dbo].[MP_PayPalTransactionItem] 
(
	[Payer] ASC
)
INCLUDE ( [TransactionId],
[Created],
[NetAmountAmount],
[Type],
[Status]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, 
IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, 
ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

/****** Object:  Index [IX_MP_ServiceLog_CustomerId]    Script Date: 01/21/2013 15:56:03 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_ServiceLog]') AND name = N'IX_MP_ServiceLog_CustomerId')
DROP INDEX [IX_MP_ServiceLog_CustomerId] ON [dbo].[MP_ServiceLog] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_ServiceLog_CustomerId]    Script Date: 01/21/2013 15:56:03 ******/
CREATE NONCLUSTERED INDEX [IX_MP_ServiceLog_CustomerId] ON [dbo].[MP_ServiceLog] 
(
	[CustomerId] ASC
)
INCLUDE ( [ServiceType],
[InsertDate],
[RequestData],
[ResponseData]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

/****** Object:  Index [IX_LoanAgreement_Loan]    Script Date: 01/21/2013 16:01:50 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LoanAgreement]') AND name = N'IX_LoanAgreement_Loan')
DROP INDEX [IX_LoanAgreement_Loan] ON [dbo].[LoanAgreement] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_LoanAgreement_Loan]    Script Date: 01/21/2013 16:01:50 ******/
CREATE NONCLUSTERED INDEX [IX_LoanAgreement_Loan] ON [dbo].[LoanAgreement] 
(
	[LoanId] ASC
)
INCLUDE ( [Name],
[Template],
[FilePath],
[ZohoId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

