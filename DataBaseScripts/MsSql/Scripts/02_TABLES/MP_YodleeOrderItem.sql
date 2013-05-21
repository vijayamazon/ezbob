IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_YodleeOrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_YodleeOrderItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_YodleeOrderItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[isSeidFromDataSource] [int] NULL,
	[isSeidFromDataSourceSpecified] [bit] NULL,
	[isSeidMod] [int] NULL,
	[isSeidModSpecified] [bit] NULL,
	[acctTypeId] [int] NULL,
	[acctTypeIdSpecified] [bit] NULL,
	[acctType] [nvarchar](300) NULL,
	[localizedAcctType] [nvarchar](300) NULL,
	[srcElementId] [nvarchar](300) NULL,
	[individualInformationId] [int] NULL,
	[individualInformationIdSpecified] [bit] NULL,
	[bankAccountId] [int] NULL,
	[bankAccountIdSpecified] [bit] NULL,
	[customName] [nvarchar](300) NULL,
	[customDescription] [nvarchar](300) NULL,
	[isDeleted] [int] NULL,
	[isDeletedSpecified] [bit] NULL,
	[lastUpdated] [int] NULL,
	[lastUpdatedSpecified] [bit] NULL,
	[hasDetails] [int] NULL,
	[hasDetailsSpecified] [bit] NULL,
	[interestRate] [float] NULL,
	[interestRateSpecified] [bit] NULL,
	[accountNumber] [nvarchar](300) NULL,
	[link] [nvarchar](300) NULL,
	[accountHolder] [nvarchar](300) NULL,
	[tranListToDate] [datetime] NULL,
	[tranListFromDate] [datetime] NULL,
	[availableBalance] [float] NULL,
	[availableBalanceCurrency] [nvarchar](3) NULL,
	[currentBalance] [float] NULL,
	[currentBalanceCurrency] [nvarchar](3) NULL,
	[interestEarnedYtd] [float] NULL,
	[interestEarnedYtdCurrency] [nvarchar](3) NULL,
	[prevYrInterest] [float] NULL,
	[prevYrInterestCurrency] [nvarchar](3) NULL,
	[overdraftProtection] [float] NULL,
	[overdraftProtectionCurrency] [nvarchar](3) NULL,
	[term] [nvarchar](300) NULL,
	[accountName] [nvarchar](300) NULL,
	[annualPercentYield] [float] NULL,
	[annualPercentYieldSpecified] [bit] NULL,
	[routingNumber] [nvarchar](300) NULL,
	[maturityDate] [datetime] NULL,
	[asOfDate] [datetime] NULL,
	[accountNicknameAtSrcSite] [nvarchar](300) NULL,
	[isPaperlessStmtOn] [int] NULL,
	[isPaperlessStmtOnSpecified] [bit] NULL,
	[siteAccountStatusSpecified] [bit] NULL,
	[created] [int] NULL,
	[createdSpecified] [bit] NULL,
	[nomineeName] [nvarchar](300) NULL,
	[secondaryAccountHolderName] [nvarchar](300) NULL,
	[accountOpenDate] [datetime] NULL,
	[accountCloseDate] [datetime] NULL,
	[maturityAmount] [float] NULL,
	[maturityAmountCurrency] [nvarchar](3) NULL,
	[taxesWithheldYtd] [float] NULL,
	[taxesWithheldYtdCurrency] [nvarchar](3) NULL,
	[taxesPaidYtd] [float] NULL,
	[taxesPaidYtdCurrency] [nvarchar](3) NULL,
	[budgetBalance] [float] NULL,
	[budgetBalanceCurrency] [nvarchar](3) NULL,
	[straightBalance] [float] NULL,
	[straightBalanceCurrency] [nvarchar](3) NULL,
	[accountClassificationSpecified] [bit] NULL,
 CONSTRAINT [PK_MP_YodleeOrderItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MP_YodleeOrderItemOrderId] ON [dbo].[MP_YodleeOrderItem] 
(
	[OrderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
