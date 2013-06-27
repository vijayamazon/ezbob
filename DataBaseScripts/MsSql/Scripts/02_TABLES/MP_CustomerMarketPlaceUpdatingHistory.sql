IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlaceUpdatingHistory]') AND type in (N'U'))
DROP TABLE [dbo].[MP_CustomerMarketPlaceUpdatingHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketPlaceUpdatingHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[UpdatingStart] [datetime] NOT NULL,
	[UpdatingEnd] [datetime] NULL,
	[Error] [nvarchar](max) NULL,
	[UpdatingTimePassInSeconds]  AS (datediff(second,[UpdatingStart],[UpdatingEnd])),
 CONSTRAINT [PK_MP_CustomerMarketPlaceUpdatingHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart] ON [dbo].[MP_CustomerMarketPlaceUpdatingHistory] 
(
	[UpdatingStart] ASC
)
INCLUDE ( [CustomerMarketPlaceId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
