IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Askville]') AND type in (N'U'))
DROP TABLE [dbo].[Askville]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Askville](
	[Guid] [nvarchar](200) NULL,
	[MarketPlaceId] [int] NULL,
	[isPassed] [bit] NULL,
	[Status] [nvarchar](200) NULL,
	[SendStatus] [nvarchar](50) NULL,
	[MessageBody] [nvarchar](max) NULL,
	[CreationDate] [datetime] NULL
) ON [PRIMARY]
GO
