IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemCalendar_Day]') AND type in (N'U'))
DROP TABLE [dbo].[SystemCalendar_Day]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemCalendar_Day](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DayOfWeek] [nvarchar](255) NOT NULL,
	[IsWorkDay] [bit] NOT NULL,
	[BeginsAt] [datetime] NULL,
	[Duration] [int] NULL,
	[LunchBeginsAt] [datetime] NULL,
	[LunchDuration] [int] NULL,
	[HostCalendarId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
