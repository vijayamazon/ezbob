IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentRollover]') AND type in (N'U'))
DROP TABLE [dbo].[PaymentRollover]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentRollover](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoanScheduleId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CreatorName] [nvarchar](256) NULL,
	[Payment] [decimal](18, 0) NOT NULL,
	[PaymentDueDate] [datetime] NULL,
	[PaymentNewDate] [datetime] NULL,
	[ExpiryDate] [datetime] NULL,
	[CustomerConfirmationDate] [datetime] NULL,
	[PaidPaymentAmount] [decimal](18, 0) NULL,
	[Status] [varchar](50) NULL,
	[MounthCount] [int] NOT NULL,
 CONSTRAINT [PK_MP_PaymentRollover] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PaymentRollover] ADD  DEFAULT ((1)) FOR [MounthCount]
GO
