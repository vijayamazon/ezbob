IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanTransaction]') AND type in (N'U'))
DROP TABLE [dbo].[LoanTransaction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](100) NOT NULL,
	[PostDate] [datetime] NOT NULL,
	[Amount] [numeric](18, 2) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[LoanId] [int] NOT NULL,
	[Status] [nvarchar](50) NULL,
	[TrackingNumber] [nvarchar](100) NULL,
	[PacnetStatus] [nvarchar](1000) NULL,
	[PaypointId] [nvarchar](1000) NULL,
	[IP] [nvarchar](100) NULL,
	[Principal] [numeric](18, 2) NULL,
	[Interest] [numeric](18, 2) NULL,
	[Fees] [numeric](18, 2) NULL,
	[Balance] [numeric](18, 2) NULL,
	[RefNumber] [nchar](14) NULL,
	[LoanRepayment] [numeric](18, 4) NULL,
	[Rollover] [numeric](18, 4) NULL,
	[InterestOnly] [bit] NULL,
 CONSTRAINT [PK_LoanTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
