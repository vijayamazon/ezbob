IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_TransactionItem2_MP_Currency]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem2]'))
ALTER TABLE [dbo].[MP_PayPalTransactionItem2] DROP CONSTRAINT [FK_MP_TransactionItem2_MP_Currency]
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2]  WITH CHECK ADD  CONSTRAINT [FK_MP_TransactionItem2_MP_Currency] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[MP_Currency] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2] CHECK CONSTRAINT [FK_MP_TransactionItem2_MP_Currency]
GO
