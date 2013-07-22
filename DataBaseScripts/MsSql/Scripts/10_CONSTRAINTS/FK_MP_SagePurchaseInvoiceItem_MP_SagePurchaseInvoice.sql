IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_SagePurchaseInvoiceItem_MP_SagePurchaseInvoice]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_SagePurchaseInvoiceItem]'))
ALTER TABLE [dbo].[MP_SagePurchaseInvoiceItem] DROP CONSTRAINT [FK_MP_SagePurchaseInvoiceItem_MP_SagePurchaseInvoice]
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_SagePurchaseInvoiceItem_MP_SagePurchaseInvoice] FOREIGN KEY([PurchaseInvoiceId])
REFERENCES [dbo].[MP_SagePurchaseInvoice] ([Id])
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoiceItem] CHECK CONSTRAINT [FK_MP_SagePurchaseInvoiceItem_MP_SagePurchaseInvoice]
GO
