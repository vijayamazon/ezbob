IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanTransactionMethod]') AND type in (N'U'))
DROP TABLE [dbo].[LoanTransactionMethod]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanTransactionMethod](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[DisplaySort] [int] NOT NULL,
 CONSTRAINT [PK_LoanTransactionMethod] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
