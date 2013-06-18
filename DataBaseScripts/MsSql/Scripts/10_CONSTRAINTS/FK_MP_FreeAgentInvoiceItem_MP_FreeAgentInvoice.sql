IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_FreeAgentInvoiceItem_MP_FreeAgentInvoice]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_FreeAgentInvoiceItem]'))
ALTER TABLE [dbo].[MP_FreeAgentInvoiceItem] DROP CONSTRAINT [FK_MP_FreeAgentInvoiceItem_MP_FreeAgentInvoice]
GO
ALTER TABLE [dbo].[MP_FreeAgentInvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_FreeAgentInvoiceItem_MP_FreeAgentInvoice] FOREIGN KEY([InvoiceId])
REFERENCES [dbo].[MP_FreeAgentInvoice] ([Id])
GO
ALTER TABLE [dbo].[MP_FreeAgentInvoiceItem] CHECK CONSTRAINT [FK_MP_FreeAgentInvoiceItem_MP_FreeAgentInvoice]
GO
