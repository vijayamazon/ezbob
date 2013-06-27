IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanAgreement]') AND type in (N'U'))
DROP TABLE [dbo].[LoanAgreement]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanAgreement](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Template] [nvarchar](max) NOT NULL,
	[LoanId] [int] NULL,
	[FilePath] [nvarchar](400) NULL,
	[ZohoId] [nvarchar](100) NULL,
 CONSTRAINT [PK_LoanAgreement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_LoanAgreement_Loan] ON [dbo].[LoanAgreement] 
(
	[LoanId] ASC
)
INCLUDE ( [Name],
[Template],
[FilePath],
[ZohoId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
