IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExperianDefaultAccountsData]') AND type in (N'U'))
DROP TABLE [dbo].[ExperianDefaultAccountsData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperianDefaultAccountsData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ServiceLogId] [bigint] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[AccountType] [varchar](3) NULL,
	[DefMonth] [datetime] NULL,
	[Balance] [int] NULL,
	[CurrentDefBalance] [int] NULL
) ON [PRIMARY]
GO
