ALTER TABLE dbo.CashRequests ADD
	[RepaymentPeriod] [int] NOT NULL
	CONSTRAINT [DF_CashRequests_RepaymentPeriod]  DEFAULT ((3))
GO