IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Security_UserRoleRelation_Security_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Security_UserRoleRelation]'))
ALTER TABLE [dbo].[Security_UserRoleRelation] DROP CONSTRAINT [FK_Security_UserRoleRelation_Security_User]
GO
ALTER TABLE [dbo].[Security_UserRoleRelation]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_UserRoleRelation_Security_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Security_User] ([UserId])
GO
ALTER TABLE [dbo].[Security_UserRoleRelation] CHECK CONSTRAINT [FK_Security_UserRoleRelation_Security_User]
GO
