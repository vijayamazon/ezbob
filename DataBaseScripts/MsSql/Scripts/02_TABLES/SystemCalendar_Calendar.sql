IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemCalendar_Calendar]') AND type in (N'U'))
DROP TABLE [dbo].[SystemCalendar_Calendar]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemCalendar_Calendar](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IsDeleted] [int] NULL,
	[IsBase] [bit] NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[UserId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[TerminationDate] [datetime] NULL,
	[SignedDocument] [nvarchar](max) NULL,
	[SignedDocumentDelete] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[IsDeleted] ASC,
	[DisplayName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
