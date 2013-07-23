IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND type in (N'U'))
DROP TABLE [dbo].[Customer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customer](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[CreditResult] [nvarchar](max) NULL,
	[CreditSum] [decimal](18, 0) NULL,
	[GreetingMailSent] [int] NULL,
	[GreetingMailSentDate] [datetime] NULL,
	[Status] [nvarchar](250) NULL,
	[IsSuccessfullyRegistered] [bit] NULL,
	[AccountNumber] [nvarchar](8) NULL,
	[SortCode] [nvarchar](8) NULL,
	[FirstName] [nvarchar](250) NULL,
	[MiddleInitial] [nvarchar](250) NULL,
	[Surname] [nvarchar](250) NULL,
	[DateOfBirth] [datetime] NULL,
	[TimeAtAddress] [int] NULL,
	[ResidentialStatus] [nvarchar](250) NULL,
	[LimitedCompanyNumber] [nvarchar](255) NULL,
	[LimitedCompanyName] [nvarchar](250) NULL,
	[LimitedTimeAtAddress] [int] NULL,
	[LimitedConsentToSearch] [bit] NULL,
	[NonLimitedCompanyName] [nvarchar](250) NULL,
	[NonLimitedTimeInBusiness] [nvarchar](250) NULL,
	[NonLimitedTimeAtAddress] [int] NULL,
	[NonLimitedConsentToSearch] [bit] NULL,
	[ApplyForLoan] [datetime] NULL,
	[MedalType] [nvarchar](50) NULL,
	[PayPointTransactionId] [nvarchar](250) NULL,
	[DateEscalated] [datetime] NULL,
	[UnderwriterName] [varchar](200) NULL,
	[ManagerName] [varchar](200) NULL,
	[EscalationReason] [varchar](200) NULL,
	[DateApproved] [datetime] NULL,
	[ApplyCount] [int] NULL,
	[RejectedReason] [varchar](200) NULL,
	[Gender] [char](1) NULL,
	[MartialStatus] [nvarchar](50) NULL,
	[TypeOfBusiness] [nvarchar](50) NULL,
	[SystemDecision] [nvarchar](50) NULL,
	[CreditCardNo] [nvarchar](50) NULL,
	[DaytimePhone] [nvarchar](50) NULL,
	[MobilePhone] [nvarchar](50) NULL,
	[LimitedBusinessPhone] [nvarchar](50) NULL,
	[NonLimitedBusinessPhone] [nvarchar](50) NULL,
	[Fullname] [nvarchar](250) NULL,
	[LimitedRefNum] [nvarchar](250) NULL,
	[NonLimitedRefNum] [nvarchar](250) NULL,
	[OverallTurnOver] [decimal](18, 0) NULL,
	[WebSiteTurnOver] [decimal](18, 0) NULL,
	[BWAResult] [nvarchar](100) NULL,
	[AMLResult] [nvarchar](100) NULL,
	[Fraud] [bit] NULL,
	[Eliminated] [bit] NULL,
	[RefNumber] [nvarchar](8) NULL,
	[PayPointErrorsCount] [int] NULL,
	[SetupFee] [decimal](18, 0) NULL,
	[Comments] [nvarchar](max) NULL,
	[Details] [nvarchar](max) NULL,
	[ValidFor] [datetime] NULL,
	[CollectionStatus] [int] NOT NULL,
	[ApprovedReason] [nchar](200) NULL,
	[ReferenceSource] [nvarchar](200) NULL,
	[EmailState] [nvarchar](100) NULL,
	[IsTest] [bit] NULL,
	[CurrentDebitCard] [int] NULL,
	[ZohoId] [nvarchar](100) NULL,
	[BankAccountType] [nvarchar](50) NULL,
	[BankAccountValidationInvalidAttempts] [int] NULL,
	[CollectionDateOfDeclaration] [datetime] NULL,
	[IsAddCollectionFee] [bit] NULL,
	[CollectionFee] [decimal](18, 0) NULL,
	[CollectionDescription] [nvarchar](50) NULL,
	[WizardStep] [int] NULL,
	[LastStartedMainStrategyId] [int] NULL,
	[LastStartedMainStrategyEndTime] [datetime] NULL,
	[PendingStatus] [nvarchar](50) NULL,
	[DateRejected] [datetime] NULL,
	[IsLoanTypeSelectionAllowed] [int] NULL,
	[ABTesting] [nvarchar](512) NULL,
	[NumApproves] [int] NOT NULL,
	[NumRejects] [int] NOT NULL,
	[SystemCalculatedSum] [decimal](18, 4) NOT NULL,
	[ManagerApprovedSum] [decimal](18, 4) NOT NULL,
	[FirstLoanDate] [datetime] NULL,
	[LastLoanDate] [datetime] NULL,
	[AmountTaken] [decimal](18, 4) NOT NULL,
	[LastLoanAmount] [decimal](18, 4) NOT NULL,
	[TotalPrincipalRepaid] [decimal](18, 4) NOT NULL,
	[LastStatus] [nvarchar](100) NULL,
	[AvoidAutomaticDescison] [bit] NOT NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Customer_Fill] ON [dbo].[Customer] 
(
	[IsSuccessfullyRegistered] ASC,
	[IsTest] ASC
)
INCLUDE ( [Id],
[CreditResult],
[FirstName],
[MiddleInitial],
[Surname],
[DateOfBirth],
[TimeAtAddress],
[ResidentialStatus],
[Gender],
[MartialStatus],
[TypeOfBusiness],
[DaytimePhone],
[MobilePhone],
[Fullname],
[OverallTurnOver],
[WebSiteTurnOver]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Customer_IsRegistered] ON [dbo].[Customer] 
(
	[IsSuccessfullyRegistered] ASC
)
INCLUDE ( [CreditResult]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Customer_RefNumber] ON [dbo].[Customer] 
(
	[RefNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_IsLoanType]  DEFAULT ((0)) FOR [IsLoanTypeSelectionAllowed]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_NumApproves]  DEFAULT ((0)) FOR [NumApproves]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_NumRejects]  DEFAULT ((0)) FOR [NumRejects]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_SystemCalculatedSum]  DEFAULT ((0)) FOR [SystemCalculatedSum]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_ManagerApprovedSum]  DEFAULT ((0)) FOR [ManagerApprovedSum]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_AmountTaken]  DEFAULT ((0)) FOR [AmountTaken]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_LastLoanAmount]  DEFAULT ((0)) FOR [LastLoanAmount]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_TotalPrincipalRepaid]  DEFAULT ((0)) FOR [TotalPrincipalRepaid]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_LastStatus]  DEFAULT ('N/A') FOR [LastStatus]
GO
ALTER TABLE [dbo].[Customer] ADD  CONSTRAINT [DF_Customer_AvoidAutomaticDescison]  DEFAULT ((0)) FOR [AvoidAutomaticDescison]
GO
