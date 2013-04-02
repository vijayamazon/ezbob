IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_APPNODESETTING_NODEID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_NodeSetting]'))
ALTER TABLE [dbo].[Application_NodeSetting] DROP CONSTRAINT [FK_APPNODESETTING_NODEID]
GO
ALTER TABLE [dbo].[Application_NodeSetting]  WITH NOCHECK ADD  CONSTRAINT [FK_APPNODESETTING_NODEID] FOREIGN KEY([NodeId])
REFERENCES [dbo].[Strategy_Node] ([NodeId])
GO
ALTER TABLE [dbo].[Application_NodeSetting] CHECK CONSTRAINT [FK_APPNODESETTING_NODEID]
GO
