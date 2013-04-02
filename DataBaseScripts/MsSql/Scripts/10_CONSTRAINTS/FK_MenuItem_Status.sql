IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MenuItem_Status]') AND parent_object_id = OBJECT_ID(N'[dbo].[MenuItem_Status_Rel]'))
ALTER TABLE [dbo].[MenuItem_Status_Rel] DROP CONSTRAINT [FK_MenuItem_Status]
GO
ALTER TABLE [dbo].[MenuItem_Status_Rel]  WITH CHECK ADD  CONSTRAINT [FK_MenuItem_Status] FOREIGN KEY([MenuItemId])
REFERENCES [dbo].[MenuItem] ([Id])
GO
ALTER TABLE [dbo].[MenuItem_Status_Rel] CHECK CONSTRAINT [FK_MenuItem_Status]
GO
