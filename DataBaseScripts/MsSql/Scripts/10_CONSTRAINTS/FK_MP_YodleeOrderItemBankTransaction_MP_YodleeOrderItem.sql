IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_YodleeOrderItemBankTransaction_MP_YodleeOrderItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_YodleeOrderItemBankTransaction]'))
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction] DROP CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeOrderItem]
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeOrderItem] FOREIGN KEY([OrderItemId])
REFERENCES [dbo].[MP_YodleeOrderItem] ([Id])
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction] CHECK CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeOrderItem]
GO
