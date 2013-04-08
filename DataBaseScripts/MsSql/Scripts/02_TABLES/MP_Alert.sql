IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_Alert]') AND type in (N'U'))
DROP TABLE [dbo].[MP_Alert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_Alert](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AlertType] [nvarchar](500) NULL,
	[AlertSeverity] [int] NULL,
	[AlertText] [nvarchar](max) NULL,
	[Status] [nvarchar](500) NULL,
	[ActionToMake] [nvarchar](max) NULL,
	[ActionDate] [datetime] NULL,
	[UserId] [int] NULL,
	[CustomerId] [int] NULL,
	[DirectorId] [int] NULL,
	[Details] [nvarchar](max) NULL,
	[StrategyStartedDate] [datetime] NULL,
 CONSTRAINT [PK_MP_Alert] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_Alert_CustId] ON [dbo].[MP_Alert] 
(
	[CustomerId] ASC
)
INCLUDE ( [StrategyStartedDate]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
