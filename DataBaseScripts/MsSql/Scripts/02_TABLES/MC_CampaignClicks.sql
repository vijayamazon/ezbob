IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MC_CampaignClicks]') AND type in (N'U'))
DROP TABLE [dbo].[MC_CampaignClicks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MC_CampaignClicks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[Title] [nvarchar](300) NULL,
	[Url] [nvarchar](300) NULL,
	[EmailsSent] [int] NULL,
	[Clicks] [int] NULL,
	[Email] [nvarchar](300) NULL,
 CONSTRAINT [PK_MC_CampaignClicks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
