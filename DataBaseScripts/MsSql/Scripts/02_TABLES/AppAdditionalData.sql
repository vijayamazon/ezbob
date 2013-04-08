IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppAdditionalData]') AND type in (N'U'))
DROP TABLE [dbo].[AppAdditionalData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppAdditionalData](
	[AppId] [int] NOT NULL,
	[Name] [nvarchar](256) NULL,
	[PassportSeries] [nvarchar](256) NULL,
	[Patronymic] [nvarchar](1024) NULL,
	[StatusId] [int] NULL,
	[Surname] [nvarchar](1024) NULL,
	[CreditProduct] [nvarchar](1024) NULL,
	[DesiredCreditSum] [decimal](18, 0) NULL,
	[ActualCreditSum] [decimal](18, 0) NULL,
	[ReadOnlyNodeName] [nvarchar](1024) NULL,
	[AutoCreditTerm] [nvarchar](1024) NULL,
	[AutoCreditFirstPayment] [nvarchar](1024) NULL,
	[DecisionStatus] [nvarchar](1024) NULL,
	[CommandsList] [nchar](32) NULL,
 CONSTRAINT [PK_AppAdditionalData] PRIMARY KEY CLUSTERED 
(
	[AppId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
