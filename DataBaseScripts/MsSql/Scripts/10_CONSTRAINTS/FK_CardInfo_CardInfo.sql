IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CardInfo_CardInfo]') AND parent_object_id = OBJECT_ID(N'[dbo].[CardInfo]'))
ALTER TABLE [dbo].[CardInfo] DROP CONSTRAINT [FK_CardInfo_CardInfo]
GO
ALTER TABLE [dbo].[CardInfo]  WITH NOCHECK ADD  CONSTRAINT [FK_CardInfo_CardInfo] FOREIGN KEY([Id])
REFERENCES [dbo].[CardInfo] ([Id])
GO
ALTER TABLE [dbo].[CardInfo] CHECK CONSTRAINT [FK_CardInfo_CardInfo]
GO
