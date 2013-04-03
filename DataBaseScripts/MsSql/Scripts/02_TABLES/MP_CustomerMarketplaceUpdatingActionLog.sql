IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketplaceUpdatingActionLog]') AND type in (N'U'))
DROP TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketplaceUpdatingActionLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerMarketplaceUpdatingHistoryRecordId] [int] NULL,
	[UpdatingStart] [datetime] NULL,
	[UpdatingEnd] [datetime] NULL,
	[ActionName] [nvarchar](128) NULL,
	[ControlValueName] [nvarchar](128) NULL,
	[ControlValue] [nvarchar](max) NULL,
	[Error] [nvarchar](max) NULL,
	[UpdatingTimePassInSeconds]  AS (datediff(second,[UpdatingStart],[UpdatingEnd])),
	[ElapsedAggregateData] [bigint] NULL,
	[ElapsedRetrieveDataFromDatabase] [bigint] NULL,
	[ElapsedRetrieveDataFromExternalService] [bigint] NULL,
	[ElapsedStoreAggregatedData] [bigint] NULL,
	[ElapsedStoreDataToDatabase] [bigint] NULL,
 CONSTRAINT [PK_MP_CustomerMarketPlaceUpdatingActionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
