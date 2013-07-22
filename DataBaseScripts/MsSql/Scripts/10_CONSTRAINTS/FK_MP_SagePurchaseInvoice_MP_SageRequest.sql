IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_SagePurchaseInvoice_MP_SageRequest]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_SagePurchaseInvoice]'))
ALTER TABLE [dbo].[MP_SagePurchaseInvoice] DROP CONSTRAINT [FK_MP_SagePurchaseInvoice_MP_SageRequest]
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoice]  WITH CHECK ADD  CONSTRAINT [FK_MP_SagePurchaseInvoice_MP_SageRequest] FOREIGN KEY([RequestId])
REFERENCES [dbo].[MP_SageRequest] ([Id])
GO
ALTER TABLE [dbo].[MP_SagePurchaseInvoice] CHECK CONSTRAINT [FK_MP_SagePurchaseInvoice_MP_SageRequest]
GO
