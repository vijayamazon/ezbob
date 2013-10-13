IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_MarketplaceType]') AND type in (N'U'))
DROP TABLE [dbo].[MP_MarketplaceType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_MarketplaceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[InternalId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Active] [bit] NOT NULL,
	[IsOffline] [bit] NOT NULL,
 CONSTRAINT [PK_MarketPlace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_MP_MarketPlaceInternalId] UNIQUE NONCLUSTERED 
(
	[InternalId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MP_MarketplaceType] ADD  CONSTRAINT [DF_MarketPlace_InternalId]  DEFAULT (newid()) FOR [InternalId]
GO
ALTER TABLE [dbo].[MP_MarketplaceType] ADD  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [dbo].[MP_MarketplaceType] ADD  CONSTRAINT [DF_MarketplaceType_Offline]  DEFAULT ((0)) FOR [IsOffline]
GO
