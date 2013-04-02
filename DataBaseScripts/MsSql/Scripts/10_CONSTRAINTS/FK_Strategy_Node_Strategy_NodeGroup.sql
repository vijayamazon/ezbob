IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Strategy_Node_Strategy_NodeGroup]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_Node]'))
ALTER TABLE [dbo].[Strategy_Node] DROP CONSTRAINT [FK_Strategy_Node_Strategy_NodeGroup]
GO
ALTER TABLE [dbo].[Strategy_Node]  WITH NOCHECK ADD  CONSTRAINT [FK_Strategy_Node_Strategy_NodeGroup] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Strategy_NodeGroup] ([NodeGroupId])
GO
ALTER TABLE [dbo].[Strategy_Node] CHECK CONSTRAINT [FK_Strategy_Node_Strategy_NodeGroup]
GO
