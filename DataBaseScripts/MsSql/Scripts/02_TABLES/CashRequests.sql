IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CashRequests]') AND type in (N'U'))
DROP TABLE [dbo].[CashRequests]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CashRequests](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IdCustomer] [int] NOT NULL,
	[IdUnderwriter] [int] NULL,
	[CreationDate] [datetime] NULL,
	[SystemDecision] [nvarchar](50) NULL,
	[UnderwriterDecision] [nvarchar](50) NULL,
	[SystemDecisionDate] [datetime] NULL,
	[UnderwriterDecisionDate] [datetime] NULL,
	[EscalatedDate] [datetime] NULL,
	[SystemCalculatedSum] [int] NULL,
	[ManagerApprovedSum] [int] NULL,
	[MedalType] [nvarchar](50) NULL,
	[EscalationReason] [nvarchar](200) NULL,
	[APR] [decimal](18, 0) NULL,
	[RepaymentPeriod] [int] NOT NULL,
	[ScorePoints] [numeric](8, 3) NULL,
	[ExpirianRating] [int] NULL,
	[AnualTurnover] [int] NULL,
	[InterestRate] [decimal](18, 4) NOT NULL,
	[UseSetupFee] [int] NOT NULL,
	[EmailSendingBanned] [bit] NULL,
	[LoanTypeId] [int] NULL,
	[UnderwriterComment] [nvarchar](200) NULL,
	[HasLoans] [bit] NULL,
	[LoanTemplate] [nvarchar](max) NULL,
	[IsLoanTypeSelectionAllowed] [int] NULL,
	[DiscountPlanId] [int] NULL,
 CONSTRAINT [PK_CasheRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_CashRequests_IDCust] ON [dbo].[CashRequests] 
(
	[Id] ASC,
	[IdCustomer] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CashRequests] ADD  CONSTRAINT [DF_CashRequests_RepaymentPeriod]  DEFAULT ((3)) FOR [RepaymentPeriod]
GO
ALTER TABLE [dbo].[CashRequests] ADD  CONSTRAINT [DF_CashRequests_InterestRate]  DEFAULT ((0.06)) FOR [InterestRate]
GO
ALTER TABLE [dbo].[CashRequests] ADD  CONSTRAINT [DF_CashRequests_UseSetupFee]  DEFAULT ((0)) FOR [UseSetupFee]
GO
ALTER TABLE [dbo].[CashRequests] ADD  CONSTRAINT [DF_CashRequests_IsLoanType]  DEFAULT ((0)) FOR [IsLoanTypeSelectionAllowed]
GO
