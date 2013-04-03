IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemCalendar_BaseRelation]') AND type in (N'U'))
DROP TABLE [dbo].[SystemCalendar_BaseRelation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemCalendar_BaseRelation](
	[CalendarId] [int] NOT NULL,
	[BaseCalendarId] [int] NOT NULL
) ON [PRIMARY]
GO
