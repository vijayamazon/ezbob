IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Security_UserRoleRelation_Security_Role]') AND parent_object_id = OBJECT_ID(N'[dbo].[Security_UserRoleRelation]'))
ALTER TABLE [dbo].[Security_UserRoleRelation] DROP CONSTRAINT [FK_Security_UserRoleRelation_Security_Role]
GO
ALTER TABLE [dbo].[Security_UserRoleRelation]  WITH NOCHECK ADD  CONSTRAINT [FK_Security_UserRoleRelation_Security_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Security_Role] ([RoleId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Security_UserRoleRelation] CHECK CONSTRAINT [FK_Security_UserRoleRelation_Security_Role]
GO
