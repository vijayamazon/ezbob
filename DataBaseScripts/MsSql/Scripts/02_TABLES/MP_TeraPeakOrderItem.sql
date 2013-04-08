IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_TeraPeakOrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_TeraPeakOrderItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_TeraPeakOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeraPeakOrderId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[Revenue] [float] NULL,
	[Listings] [int] NULL,
	[Transactions] [int] NULL,
	[Successful] [int] NULL,
	[Bids] [int] NULL,
	[ItemsOffered] [int] NULL,
	[ItemsSold] [int] NULL,
	[AverageSellersPerDay] [int] NULL,
	[SuccessRate] [float] NULL,
	[RangeMarker] [int] NOT NULL,
 CONSTRAINT [PK_MP_TeraPeakOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MP_TeraPeakOrderItem] ADD  CONSTRAINT [DF_MP_TeraPeakOrderItem_RangeMarket]  DEFAULT ((0)) FOR [RangeMarker]
GO
