SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'InitArgs')
	ALTER TABLE DecisionTrace DROP COLUMN InitArgs
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'Properties')
	ALTER TABLE DecisionTrace DROP COLUMN Properties
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'Comment')
	ALTER TABLE DecisionTrace ADD Comment NVARCHAR(MAX) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'InputData')
	ALTER TABLE DecisionTrail ADD InputData NVARCHAR(MAX) NOT NULL
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'IsPrimary')
	ALTER TABLE DecisionTrace DROP COLUMN IsPrimary
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'IsPrimary')
	ALTER TABLE DecisionTrail ADD IsPrimary BIT NOT NULL
GO

ALTER TABLE DecisionTrace DROP COLUMN TimestampCounter
GO

ALTER TABLE DecisionTrace ADD TimestampCounter ROWVERSION
GO

ALTER TABLE DecisionTrail DROP COLUMN TimestampCounter
GO

ALTER TABLE DecisionTrail ADD TimestampCounter ROWVERSION
GO
