IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]') AND type in (N'U'))
DROP TABLE [dbo].[MP_CustomerMarketPlace]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketPlace](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MarketPlaceId] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[SecurityData] [varbinary](max) NOT NULL,
	[DisplayName] [nvarchar](512) NULL,
	[Created] [datetime] NULL,
	[Updated] [datetime] NULL,
	[UpdatingStart] [datetime] NULL,
	[UpdatingEnd] [datetime] NULL,
	[EliminationPassed] [bit] NULL,
	[Warning] [nvarchar](max) NULL,
	[UpdateError] [nvarchar](max) NULL,
	[UpdatingTimePassInSeconds]  AS (datediff(second,[UpdatingStart],[UpdatingEnd])),
	[TokenExpired] [int] NOT NULL,
 CONSTRAINT [PK_CustomerMarketPlace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace] 
(
	[CustomerId] ASC
)
INCLUDE ( [MarketPlaceId],
[DisplayName],
[EliminationPassed]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace_CUstId] ON [dbo].[MP_CustomerMarketPlace] 
(
	[CustomerId] ASC,
	[UpdatingEnd] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MP_CustomerMarketPlace] ADD  CONSTRAINT [DF_MP_CustomerMarketPlace_TokenExpired]  DEFAULT ((0)) FOR [TokenExpired]
GO
