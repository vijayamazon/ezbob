ALTER TABLE dbo.[CashRequests] Drop COLUMN [InterestRate]

ALTER TABLE dbo.CashRequests ADD
	InterestRate decimal(18, 4) NOT NULL CONSTRAINT DF_CashRequests_InterestRate DEFAULT 0.06
GO