/****** Object:  Index [IX_MP_PayPalTransactionItem_Type]    Script Date: 10/03/2012 11:21:41 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem]') 
AND name = N'IX_MP_PayPalTransactionItem_Type')
DROP INDEX [IX_MP_PayPalTransactionItem_Type] ON [dbo].[MP_PayPalTransactionItem] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_MP_PayPalTransactionItem_Type]    Script Date: 10/03/2012 11:21:41 ******/
CREATE NONCLUSTERED INDEX [IX_MP_PayPalTransactionItem_Type] ON [dbo].[MP_PayPalTransactionItem] 
(
	[TransactionId] ASC,
	[Type] ASC,
	[Status] ASC,
	[NetAmountAmount] ASC
)
INCLUDE ( [Payer])
GO

