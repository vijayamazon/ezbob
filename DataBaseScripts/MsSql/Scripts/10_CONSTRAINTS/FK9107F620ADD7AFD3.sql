IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK9107F620ADD7AFD3]') AND parent_object_id = OBJECT_ID(N'[dbo].[SystemCalendar_BaseRelation]'))
ALTER TABLE [dbo].[SystemCalendar_BaseRelation] DROP CONSTRAINT [FK9107F620ADD7AFD3]
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation]  WITH CHECK ADD  CONSTRAINT [FK9107F620ADD7AFD3] FOREIGN KEY([BaseCalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation] CHECK CONSTRAINT [FK9107F620ADD7AFD3]
GO
