IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketplaceUpdatingCounter]') AND type in (N'U'))
DROP TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_CustomerMarketplaceUpdatingCounter](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerMarketplaceUpdatingActionLogId] [bigint] NOT NULL,
	[Created] [datetime] NOT NULL,
	[Method] [nvarchar](256) NULL,
	[Details] [nvarchar](256) NULL,
 CONSTRAINT [PK_MP_CustomerMarketplaceUpdatingCounter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
