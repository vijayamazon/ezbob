SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'TimestampCounter')
	ALTER TABLE Customer DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'HasApprovalChance')
	ALTER TABLE Customer ADD HasApprovalChance BIT NULL
GO

ALTER TABLE Customer ADD TimestampCounter ROWVERSION
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'TimestampCounter')
	ALTER TABLE CashRequests DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'HasApprovalChance')
	ALTER TABLE CashRequests ADD HasApprovalChance BIT NULL
GO

ALTER TABLE CashRequests ADD TimestampCounter ROWVERSION
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'TimestampCounter')
	ALTER TABLE DecisionTrail DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'HasApprovalChance')
	ALTER TABLE DecisionTrail ADD HasApprovalChance BIT NULL
GO

ALTER TABLE DecisionTrail ADD TimestampCounter ROWVERSION
GO
