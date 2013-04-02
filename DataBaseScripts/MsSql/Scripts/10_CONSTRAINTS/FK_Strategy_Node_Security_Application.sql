IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Strategy_Node_Security_Application]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_Node]'))
ALTER TABLE [dbo].[Strategy_Node] DROP CONSTRAINT [FK_Strategy_Node_Security_Application]
GO
ALTER TABLE [dbo].[Strategy_Node]  WITH NOCHECK ADD  CONSTRAINT [FK_Strategy_Node_Security_Application] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Security_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Strategy_Node] CHECK CONSTRAINT [FK_Strategy_Node_Security_Application]
GO
