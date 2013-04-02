IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FKEA3BC91D4A086D8C]') AND parent_object_id = OBJECT_ID(N'[dbo].[Strategy_CalendarRelation]'))
ALTER TABLE [dbo].[Strategy_CalendarRelation] DROP CONSTRAINT [FKEA3BC91D4A086D8C]
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation]  WITH CHECK ADD  CONSTRAINT [FKEA3BC91D4A086D8C] FOREIGN KEY([CalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[Strategy_CalendarRelation] CHECK CONSTRAINT [FKEA3BC91D4A086D8C]
GO
