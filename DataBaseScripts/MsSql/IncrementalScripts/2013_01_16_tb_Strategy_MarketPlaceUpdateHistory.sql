
/****** Object:  Table [dbo].[Strategy_MarketPlaceUpdateHistory]    Script Date: 12/04/2012 14:21:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 04.12.2012
-- Description:	Update marketplaces with strategy
-- =============================================
*/

CREATE TABLE [dbo].[Strategy_MarketPlaceUpdateHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketPlaceId] [int] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL
) ON [PRIMARY]

GO


