IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_SageSalesInvoiceItem_MP_SageInvoice]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_SageSalesInvoiceItem]'))
ALTER TABLE [dbo].[MP_SageSalesInvoiceItem] DROP CONSTRAINT [FK_MP_SageSalesInvoiceItem_MP_SageInvoice]
GO
ALTER TABLE [dbo].[MP_SageSalesInvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_SageSalesInvoiceItem_MP_SageInvoice] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[MP_SageSalesInvoice] ([Id])
GO
ALTER TABLE [dbo].[MP_SageSalesInvoiceItem] CHECK CONSTRAINT [FK_MP_SageSalesInvoiceItem_MP_SageInvoice]
GO
