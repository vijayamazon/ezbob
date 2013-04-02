IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FKAFEECA076E0ADFD1]') AND parent_object_id = OBJECT_ID(N'[dbo].[SystemCalendar_Day]'))
ALTER TABLE [dbo].[SystemCalendar_Day] DROP CONSTRAINT [FKAFEECA076E0ADFD1]
GO
ALTER TABLE [dbo].[SystemCalendar_Day]  WITH CHECK ADD  CONSTRAINT [FKAFEECA076E0ADFD1] FOREIGN KEY([HostCalendarId])
REFERENCES [dbo].[SystemCalendar_Calendar] ([Id])
GO
ALTER TABLE [dbo].[SystemCalendar_Day] CHECK CONSTRAINT [FKAFEECA076E0ADFD1]
GO
