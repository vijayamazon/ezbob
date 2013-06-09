IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanScheduleTransaction]') AND type in (N'U'))
DROP TABLE [dbo].[LoanScheduleTransaction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanScheduleTransaction](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[LoanID] [int] NOT NULL,
	[ScheduleID] [int] NOT NULL,
	[TransactionID] [int] NOT NULL,
	[Date] [datetime] NOT NULL,
	[PrincipalDelta] [numeric](18, 2) NOT NULL,
	[FeesDelta] [numeric](18, 2) NOT NULL,
	[InterestDelta] [numeric](18, 2) NOT NULL,
	[StatusBefore] [nvarchar](50) NOT NULL,
	[StatusAfter] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_LoanScheduleTransaction] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[LoanScheduleTransaction] ADD  CONSTRAINT [DF_LoanScheduleTransaction]  DEFAULT (getdate()) FOR [Date]
GO
