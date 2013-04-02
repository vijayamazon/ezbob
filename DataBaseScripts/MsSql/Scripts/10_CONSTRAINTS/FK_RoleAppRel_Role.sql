IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RoleAppRel_Role]') AND parent_object_id = OBJECT_ID(N'[dbo].[Security_RoleAppRel]'))
ALTER TABLE [dbo].[Security_RoleAppRel] DROP CONSTRAINT [FK_RoleAppRel_Role]
GO
ALTER TABLE [dbo].[Security_RoleAppRel]  WITH NOCHECK ADD  CONSTRAINT [FK_RoleAppRel_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Security_Role] ([RoleId])
GO
ALTER TABLE [dbo].[Security_RoleAppRel] CHECK CONSTRAINT [FK_RoleAppRel_Role]
GO
