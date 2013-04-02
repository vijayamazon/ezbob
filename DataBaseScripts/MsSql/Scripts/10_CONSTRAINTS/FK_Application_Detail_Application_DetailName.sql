IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Application_Detail_Application_DetailName]') AND parent_object_id = OBJECT_ID(N'[dbo].[Application_Detail]'))
ALTER TABLE [dbo].[Application_Detail] DROP CONSTRAINT [FK_Application_Detail_Application_DetailName]
GO
ALTER TABLE [dbo].[Application_Detail]  WITH NOCHECK ADD  CONSTRAINT [FK_Application_Detail_Application_DetailName] FOREIGN KEY([DetailNameId])
REFERENCES [dbo].[Application_DetailName] ([DetailNameId])
GO
ALTER TABLE [dbo].[Application_Detail] CHECK CONSTRAINT [FK_Application_Detail_Application_DetailName]
GO
