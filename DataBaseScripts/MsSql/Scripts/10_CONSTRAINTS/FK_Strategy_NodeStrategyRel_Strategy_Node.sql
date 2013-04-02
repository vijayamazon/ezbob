IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Strategy_NodeStrategyRel_Strategy_Node]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRel]'))
ALTER TABLE [dbo].[Strategy_NodeStrategyRel] DROP CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Node]
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel]  WITH CHECK ADD  CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Node] FOREIGN KEY([NodeId])
REFERENCES [dbo].[Strategy_Node] ([NodeId])
GO
ALTER TABLE [dbo].[Strategy_NodeStrategyRel] CHECK CONSTRAINT [FK_Strategy_NodeStrategyRel_Strategy_Node]
GO
