IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CardInfo]') AND type in (N'U'))
DROP TABLE [dbo].[CardInfo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CardInfo](
	[Id] [int] NOT NULL,
	[Bank] [nvarchar](1000) NULL,
	[BankBIC] [nvarchar](200) NULL,
	[Branch] [nvarchar](1000) NULL,
	[BranchBIC] [nvarchar](200) NULL,
	[ContactAddressLine1] [nvarchar](200) NULL,
	[ContactAddressLine2] [nvarchar](200) NULL,
	[ContactPostTown] [nvarchar](200) NULL,
	[ContactPostcode] [nvarchar](200) NULL,
	[ContactPhone] [nvarchar](200) NULL,
	[ContactFax] [nvarchar](200) NULL,
	[FasterPaymentsSupported] [bit] NULL,
	[CHAPSSupported] [bit] NULL,
	[SortCode] [nvarchar](20) NULL,
	[IBAN] [nvarchar](200) NULL,
	[IsDirectDebitCapable] [bit] NULL,
	[StatusInformation] [nvarchar](200) NULL,
	[CustomerId] [int] NULL,
	[BankAccount] [nvarchar](8) NULL,
	[BWAResult] [nvarchar](100) NULL,
	[BankAccountType] [nchar](150) NULL,
 CONSTRAINT [PK_CardInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
