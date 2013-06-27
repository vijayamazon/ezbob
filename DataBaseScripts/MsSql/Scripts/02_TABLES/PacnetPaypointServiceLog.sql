IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PacnetPaypointServiceLog]') AND type in (N'U'))
DROP TABLE [dbo].[PacnetPaypointServiceLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PacnetPaypointServiceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerId] [bigint] NULL,
	[InsertDate] [datetime] NULL,
	[RequestType] [nvarchar](max) NULL,
	[Status] [nvarchar](50) NULL,
	[ErrorMessage] [nvarchar](max) NULL,
 CONSTRAINT [PK_PacnetServiceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [PacnetPaypointServiceLog_CustId] ON [dbo].[PacnetPaypointServiceLog] 
(
	[CustomerId] ASC
)
INCLUDE ( [InsertDate],
[RequestType],
[Status],
[ErrorMessage]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
