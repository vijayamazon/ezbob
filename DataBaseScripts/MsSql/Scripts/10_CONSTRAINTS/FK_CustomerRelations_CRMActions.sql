IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerRelations_CRMActions]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomerRelations]'))
ALTER TABLE [dbo].[CustomerRelations] DROP CONSTRAINT [FK_CustomerRelations_CRMActions]
GO
ALTER TABLE [dbo].[CustomerRelations]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRelations_CRMActions] FOREIGN KEY([ActionId])
REFERENCES [dbo].[CRMActions] ([Id])
GO
ALTER TABLE [dbo].[CustomerRelations] CHECK CONSTRAINT [FK_CustomerRelations_CRMActions]
GO
