IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_MarketPlaceUpdateHistory]') AND type in (N'U'))
DROP TABLE [dbo].[Strategy_MarketPlaceUpdateHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategy_MarketPlaceUpdateHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketPlaceId] [int] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL
) ON [PRIMARY]
GO
