ALTER TABLE Loan ADD [RequestCashId] [bigint] NULL;
go

CREATE TABLE [dbo].[CashRequests](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IdCustomer] [int] NOT NULL,
	[IdUnderwriter] [int] NULL,
	[IdManager] [int] NULL,
	[CreationDate] [datetime] NULL,
	[SystemDecision] [nvarchar](50) NULL,
	[UnderwriterDecision] [nvarchar](50) NULL,
	[ManagerDecision] [nvarchar](50) NULL,
	[SystemDecisionDate] [datetime] NULL,
	[UnderwriterDecisionDate] [datetime] NULL,
	[ManagerDecisionDate] [datetime] NULL,
	[EscalatedDate] [datetime] NULL,
	[SystemCalculatedSum] [decimal](18, 0) NULL,
	[ManagerApprovedSum] [decimal](18, 0) NULL,
	[MedalType] [nvarchar](50) NULL,
	[EscalationReason] [nvarchar](200) NULL,
 CONSTRAINT [PK_CasheRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
