SET NOCOUNT ON
USE $(DATABASE_NAME);
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CreditApproval500Mixed](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Sex] [nvarchar](6) NOT NULL,
	[Age] [int] NOT NULL,
	[AddresTime] [int] NOT NULL,
	[MaritalStatus] [nvarchar] (10) NOT NULL,
	[Occupation] [nvarchar] (20) NOT NULL,
	[JobTime] [int] NOT NULL,
	[Checking] [nvarchar] (5) NOT NULL,
	[Savings] [nvarchar] (5) NOT NULL,
	[PaymentHistory] [int] NOT NULL,
	[HomeOwnership] [nvarchar] (5) NOT NULL,
	[FinRatio1] [int] NOT NULL,
	[FinRatio2] [int] NOT NULL,
	[CreditRisk] [nvarchar] (5) NOT NULL,
	[WithNull] [int] NULL,
)
 ON [PRIMARY]
GO