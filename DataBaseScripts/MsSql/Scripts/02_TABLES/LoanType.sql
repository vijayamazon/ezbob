IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanType]') AND type in (N'U'))
DROP TABLE [dbo].[LoanType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanType](
	[Id] [int] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsDefault] [bit] NOT NULL,
	[RepaymentPeriod] [int] NULL,
 CONSTRAINT [PK_LoanType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
