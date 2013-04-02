IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_APPNODESETTING_APPID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_NodeSetting]'))
ALTER TABLE [dbo].[Application_NodeSetting] DROP CONSTRAINT [FK_APPNODESETTING_APPID]
GO
ALTER TABLE [dbo].[Application_NodeSetting]  WITH NOCHECK ADD  CONSTRAINT [FK_APPNODESETTING_APPID] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Application_Application] ([ApplicationId])
GO
ALTER TABLE [dbo].[Application_NodeSetting] CHECK CONSTRAINT [FK_APPNODESETTING_APPID]
GO
