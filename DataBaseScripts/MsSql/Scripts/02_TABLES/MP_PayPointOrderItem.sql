IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPointOrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_PayPointOrderItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_PayPointOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NULL,
	[acquirer] [varchar](300) NULL,
	[amount] [decimal](18, 0) NULL,
	[auth_code] [varchar](300) NULL,
	[authorised] [varchar](300) NULL,
	[card_type] [varchar](300) NULL,
	[cid] [varchar](300) NULL,
	[classType] [varchar](300) NULL,
	[company_no] [varchar](300) NULL,
	[country] [varchar](300) NULL,
	[currency] [varchar](300) NULL,
	[cv2avs] [varchar](300) NULL,
	[date] [datetime] NULL,
	[deferred] [varchar](300) NULL,
	[emvValue] [varchar](300) NULL,
	[ExpiryDate] [datetime] NULL,
	[fraud_code] [varchar](300) NULL,
	[FraudScore] [varchar](300) NULL,
	[ip] [varchar](300) NULL,
	[lastfive] [varchar](300) NULL,
	[merchant_no] [varchar](300) NULL,
	[message] [varchar](300) NULL,
	[MessageType] [varchar](300) NULL,
	[mid] [varchar](300) NULL,
	[name] [varchar](300) NULL,
	[options] [varchar](300) NULL,
	[start_date] [datetime] NULL,
	[status] [varchar](300) NULL,
	[tid] [varchar](300) NULL,
	[trans_id] [varchar](300) NULL,
 CONSTRAINT [PK_MP_PayPointOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
