IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Application_Setting_ApplicationDetailName1]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_Setting]'))
ALTER TABLE [dbo].[Application_Setting] DROP CONSTRAINT [FK_Application_Setting_ApplicationDetailName1]
GO
ALTER TABLE [dbo].[Application_Setting]  WITH NOCHECK ADD  CONSTRAINT [FK_Application_Setting_ApplicationDetailName1] FOREIGN KEY([Param1DetailNameId])
REFERENCES [dbo].[Application_DetailName] ([DetailNameId])
GO
ALTER TABLE [dbo].[Application_Setting] CHECK CONSTRAINT [FK_Application_Setting_ApplicationDetailName1]
GO
