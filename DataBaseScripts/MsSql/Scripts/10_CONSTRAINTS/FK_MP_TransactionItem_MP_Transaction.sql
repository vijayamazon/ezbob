IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_TransactionItem_MP_Transaction]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem]'))
ALTER TABLE [dbo].[MP_PayPalTransactionItem] DROP CONSTRAINT [FK_MP_TransactionItem_MP_Transaction]
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_TransactionItem_MP_Transaction] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[MP_PayPalTransaction] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem] CHECK CONSTRAINT [FK_MP_TransactionItem_MP_Transaction]
GO
