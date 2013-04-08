IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanCharges]') AND type in (N'U'))
DROP TABLE [dbo].[LoanCharges]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanCharges](
	[Id] [int] NOT NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[LoanId] [int] NOT NULL,
	[ConfigurationVariableId] [int] NULL,
	[Date] [datetime] NULL,
	[AmountPaid] [decimal](18, 4) NULL,
	[State] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_LoanCharges] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
