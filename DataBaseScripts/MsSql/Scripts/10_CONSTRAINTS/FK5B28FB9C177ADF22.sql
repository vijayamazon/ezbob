IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK5B28FB9C177ADF22]') AND parent_object_id = OBJECT_ID(N'[dbo].[SystemCalendar_Entry]'))
ALTER TABLE [dbo].[SystemCalendar_Entry] DROP CONSTRAINT [FK5B28FB9C177ADF22]
GO
ALTER TABLE [dbo].[SystemCalendar_Entry]  WITH CHECK ADD  CONSTRAINT [FK5B28FB9C177ADF22] FOREIGN KEY([HostEntryId])
REFERENCES [dbo].[SystemCalendar_Entry] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_Entry] CHECK CONSTRAINT [FK5B28FB9C177ADF22]
GO
