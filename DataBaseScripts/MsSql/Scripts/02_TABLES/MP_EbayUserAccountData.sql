IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_EbayUserAccountData]') AND type in (N'U'))
DROP TABLE [dbo].[MP_EbayUserAccountData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_EbayUserAccountData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[PastDue] [bit] NULL,
	[CurrentBalance] [float] NULL,
	[CreditCardModifyDate] [datetime] NULL,
	[CreditCardInfo] [nvarchar](max) NULL,
	[CreditCardExpiration] [datetime] NULL,
	[BankModifyDate] [datetime] NULL,
	[AccountState] [nvarchar](50) NULL,
	[AmountPastDueCurrency] [nvarchar](50) NULL,
	[AmountPastDueAmount] [float] NULL,
	[BankAccountInfo] [nvarchar](max) NULL,
	[AccountId] [nvarchar](max) NULL,
	[Currency] [nvarchar](50) NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NULL,
 CONSTRAINT [PK_MP_EbayUserAccountData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_EbayUserAccountDataCreatedDateIncludeMUI] ON [dbo].[MP_EbayUserAccountData] 
(
	[Created] DESC
)
INCLUDE ( [CustomerMarketPlaceId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
