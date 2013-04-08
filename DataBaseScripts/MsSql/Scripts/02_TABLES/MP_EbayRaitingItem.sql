IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayRaitingItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayRaitingItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayRaitingItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EbayFeedbackId] [int] NOT NULL,
	[TimePeriodId] [int] NOT NULL,
	[CommunicationCount] [int] NULL,
	[Communication] [float] NULL,
	[ItemAsDescribedCount] [int] NULL,
	[ItemAsDescribed] [float] NULL,
	[ShippingTimeCount] [int] NULL,
	[ShippingTime] [float] NULL,
	[ShippingAndHandlingChargesCount] [int] NULL,
	[ShippingAndHandlingCharges] [float] NULL,
 CONSTRAINT [PK_MP_EbayRaitingItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
