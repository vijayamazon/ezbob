IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_BE_Node]') AND parent_object_id = OBJECT_ID(N'[dbo].[BusinessEntity_NodeRel]'))
ALTER TABLE [dbo].[BusinessEntity_NodeRel] DROP CONSTRAINT [FK_BE_Node]
GO
ALTER TABLE [dbo].[BusinessEntity_NodeRel]  WITH CHECK ADD  CONSTRAINT [FK_BE_Node] FOREIGN KEY([NodeId])
REFERENCES [dbo].[Strategy_Node] ([NodeId])
GO
ALTER TABLE [dbo].[BusinessEntity_NodeRel] CHECK CONSTRAINT [FK_BE_Node]
GO
