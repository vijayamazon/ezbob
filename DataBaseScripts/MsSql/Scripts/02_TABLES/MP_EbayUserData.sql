IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayUserData]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayUserData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[UserID] [nvarchar](256) NULL,
	[BillingEmail] [nvarchar](256) NULL,
	[eBayGoodStanding] [bit] NULL,
	[EIASToken] [nvarchar](256) NULL,
	[EMail] [nvarchar](256) NULL,
	[FeedbackPrivate] [bit] NULL,
	[FeedbackScore] [int] NULL,
	[FeedbackRatingStar] [nvarchar](50) NULL,
	[IdVerified] [bit] NULL,
	[NewUser] [bit] NULL,
	[PayPalAccountStatus] [nvarchar](50) NULL,
	[PayPalAccountType] [nvarchar](50) NULL,
	[QualifiesForSelling] [bit] NULL,
	[RegistrationAddressId] [int] NULL,
	[RegistrationDate] [datetime] NULL,
	[SellerInfoQualifiesForB2BVAT] [bit] NULL,
	[SellerInfoSellerBusinessType] [nvarchar](50) NULL,
	[SellerInfoSellerPaymentAddressId] [int] NULL,
	[SellerInfoStoreOwner] [bit] NULL,
	[SellerInfoStoreSite] [nvarchar](max) NULL,
	[SellerInfoStoreURL] [nvarchar](max) NULL,
	[SellerInfoTopRatedSeller] [bit] NULL,
	[SellerInfoTopRatedProgram] [nvarchar](max) NULL,
	[Site] [nvarchar](max) NULL,
	[SkypeID] [nvarchar](256) NULL,
	[IDChanged] [bit] NULL,
	[IDLastChanged] [datetime] NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_EbayUserData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_EbayUserData_CreatedDateIncludeUMI] ON [dbo].[MP_EbayUserData] 
(
	[Created] DESC
)
INCLUDE ( [CustomerMarketPlaceId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
