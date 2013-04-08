IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DecisionHistory]') AND type in (N'U'))
DROP TABLE [dbo].[DecisionHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DecisionHistory](
	[Id] [int] NOT NULL,
	[Action] [nvarchar](50) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Comment] [nvarchar](2000) NULL,
	[UnderwriterId] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[CashRequestId] [int] NULL,
	[LoanTypeId] [int] NULL,
 CONSTRAINT [PK_DecisionHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
