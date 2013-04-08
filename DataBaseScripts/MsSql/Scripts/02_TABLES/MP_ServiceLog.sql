IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_ServiceLog]') AND type in (N'U'))
DROP TABLE [dbo].[MP_ServiceLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ServiceLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ServiceType] [nvarchar](500) NULL,
	[InsertDate] [datetime] NULL,
	[RequestData] [nvarchar](max) NULL,
	[ResponseData] [nvarchar](max) NULL,
	[CustomerId] [bigint] NULL,
 CONSTRAINT [PK_MP_ServiceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_ServiceLog_CustomerId] ON [dbo].[MP_ServiceLog] 
(
	[CustomerId] ASC
)
INCLUDE ( [ServiceType],
[InsertDate],
[RequestData],
[ResponseData]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
