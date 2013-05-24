CREATE TABLE [dbo].[ExperianDefaultAccount](
	[Id] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[DateAdded] [datetime] NOT NULL,
	[AccountType] [nvarchar](100) NULL,
	[Date] [datetime] NULL,
	[DelinquencyType] [nvarchar](100) NULL,
 CONSTRAINT [PK_ExperianDefaultAccounts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO