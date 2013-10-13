IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerRequestedLoan]') AND type in (N'U'))
DROP TABLE [dbo].[CustomerRequestedLoan]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerRequestedLoan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[Amount] [decimal](18, 0) NULL,
	[ReasonId] [int] NULL,
	[OtherReason] [nvarchar](300) NULL,
	[SourceOfRepaymentId] [int] NULL,
	[OtherSourceOfRepayment] [nvarchar](300) NULL,
 CONSTRAINT [PK_CustomerRequestedLoan] PRIMARY KEY CLUSTERED 
(
	[CustomerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CustomerRequestedLoan] ADD  DEFAULT (getdate()) FOR [Created]
GO
