IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanInterestFreeze]') AND type in (N'U'))
DROP TABLE [dbo].[LoanInterestFreeze]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanInterestFreeze](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoanId] [int] NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[InterestRate] [decimal](18, 4) NOT NULL,
	[ActivationDate] [datetime] NOT NULL,
	[DeactivationDate] [datetime] NULL,
 CONSTRAINT [PK_LoanInterestFreeze] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[LoanInterestFreeze] ADD  CONSTRAINT [DF_LoanInterestFreeze_Active]  DEFAULT (getdate()) FOR [ActivationDate]
GO
