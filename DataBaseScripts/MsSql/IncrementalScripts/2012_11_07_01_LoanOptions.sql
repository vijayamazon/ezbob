IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanOptions]') AND type in (N'U'))
DROP TABLE [dbo].[LoanOptions]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanOptions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoanId] [int] NULL,
	[AutoPayment] [bit] NULL,
	[ReductionFee] [bit] NULL,
	[DailyReinder] [bit] NULL,
	[CaisAccountStatus] [nvarchar](50) NULL,
	[StopSendingEmails] [bit] NULL,
 CONSTRAINT [PK_CustomerOptions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
