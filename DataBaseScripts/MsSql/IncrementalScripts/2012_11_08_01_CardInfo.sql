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
	[StatusInformation] [nvarchar](200) NULL
) ON [PRIMARY]

GO

ALTER TABLE dbo.Customer ADD
	CurrentDebitCard int NULL
GO