IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Status_MenuItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[MenuItem_Status_Rel]'))
ALTER TABLE [dbo].[MenuItem_Status_Rel] DROP CONSTRAINT [FK_Status_MenuItem]
GO
ALTER TABLE [dbo].[MenuItem_Status_Rel]  WITH CHECK ADD  CONSTRAINT [FK_Status_MenuItem] FOREIGN KEY([StatusId])
REFERENCES [dbo].[AppStatus] ([Id])
GO
ALTER TABLE [dbo].[MenuItem_Status_Rel] CHECK CONSTRAINT [FK_Status_MenuItem]
GO
