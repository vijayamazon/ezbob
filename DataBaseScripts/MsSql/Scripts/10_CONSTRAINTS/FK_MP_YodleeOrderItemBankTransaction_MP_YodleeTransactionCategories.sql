IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_YodleeOrderItemBankTransaction]'))
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction] DROP CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories]
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction]  WITH CHECK ADD  CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories] FOREIGN KEY([transactionCategoryId])
REFERENCES [dbo].[MP_YodleeTransactionCategories] ([CategoryId])
GO
ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction] CHECK CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories]
GO
