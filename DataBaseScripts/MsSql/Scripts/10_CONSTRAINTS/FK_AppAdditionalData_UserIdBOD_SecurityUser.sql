IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AppAdditionalData_UserIdBOD_SecurityUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[AppAdditionalData]'))
ALTER TABLE [dbo].[AppAdditionalData] DROP CONSTRAINT [FK_AppAdditionalData_UserIdBOD_SecurityUser]
GO
