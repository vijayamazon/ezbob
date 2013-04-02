IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK9107F6204A086D8C]') AND parent_object_id = OBJECT_ID(N'[dbo].[SystemCalendar_BaseRelation]'))
ALTER TABLE [dbo].[SystemCalendar_BaseRelation] DROP CONSTRAINT [FK9107F6204A086D8C]
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation]  WITH CHECK ADD  CONSTRAINT [FK9107F6204A086D8C] FOREIGN KEY([CalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_BaseRelation] CHECK CONSTRAINT [FK9107F6204A086D8C]
GO
