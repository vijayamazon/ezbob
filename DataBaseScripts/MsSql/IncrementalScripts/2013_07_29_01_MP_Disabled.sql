ALTER TABLE dbo.MP_CustomerMarketPlace ADD
	Disabled bit NULL
GO

ALTER TABLE dbo.MP_CustomerMarketPlace SET (LOCK_ESCALATION = TABLE)
GO