ALTER TABLE dbo.CashRequests ADD
	UseSetupFee int NOT NULL CONSTRAINT DF_CashRequests_UseSetupFee DEFAULT 0
GO