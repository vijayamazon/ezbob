IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CustomerRelations_CRMStatuses]') AND parent_object_id = OBJECT_ID(N'[dbo].[CustomerRelations]'))
ALTER TABLE [dbo].[CustomerRelations] DROP CONSTRAINT [FK_CustomerRelations_CRMStatuses]
GO
ALTER TABLE [dbo].[CustomerRelations]  WITH CHECK ADD  CONSTRAINT [FK_CustomerRelations_CRMStatuses] FOREIGN KEY([StatusId])
REFERENCES [dbo].[CRMStatuses] ([Id])
GO
ALTER TABLE [dbo].[CustomerRelations] CHECK CONSTRAINT [FK_CustomerRelations_CRMStatuses]
GO
