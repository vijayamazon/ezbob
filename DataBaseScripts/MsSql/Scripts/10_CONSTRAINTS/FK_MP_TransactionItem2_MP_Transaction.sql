IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_TransactionItem2_MP_Transaction]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransactionItem2]'))
ALTER TABLE [dbo].[MP_PayPalTransactionItem2] DROP CONSTRAINT [FK_MP_TransactionItem2_MP_Transaction]
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2]  WITH CHECK ADD  CONSTRAINT [FK_MP_TransactionItem2_MP_Transaction] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[MP_PayPalTransaction] ([Id])
GO
ALTER TABLE [dbo].[MP_PayPalTransactionItem2] CHECK CONSTRAINT [FK_MP_TransactionItem2_MP_Transaction]
GO
