IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerLoyaltyProgram]') AND type in (N'U'))
DROP TABLE [dbo].[CustomerLoyaltyProgram]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerLoyaltyProgram](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[CustomerMarketPlaceID] [int] NULL,
	[LoanID] [int] NULL,
	[LoanScheduleID] [int] NULL,
	[ActionID] [int] NOT NULL,
	[ActionDate] [datetime] NOT NULL,
	[EarnedPoints] [numeric](29, 0) NOT NULL,
 CONSTRAINT [PK_CustomerLoyaltyProgram] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_CustomerLoyaltyProgramCustomerId] ON [dbo].[CustomerLoyaltyProgram] 
(
	[CustomerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CustomerLoyaltyProgram] ADD  CONSTRAINT [DF_CustomerLoyaltyProgramDate]  DEFAULT (getdate()) FOR [ActionDate]
GO
