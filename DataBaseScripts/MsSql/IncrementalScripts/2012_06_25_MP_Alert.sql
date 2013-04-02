CREATE TABLE [dbo].[MP_Alert](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AlertType] nvarchar(500) NULL,
	[AlertSeverity] [int] NULL,
	[AlertText] nvarchar(max) NULL,
	[Status] nvarchar(500) NULL,
	[ActionToMake] nvarchar(max) NULL,
	[ActionDate] [datetime] NULL,
	[UserId] [int] NULL,
 CONSTRAINT [PK_MP_Alert] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]