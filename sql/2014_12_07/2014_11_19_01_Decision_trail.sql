SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrace') AND name = 'HasLockedDecision')
	ALTER TABLE DecisionTrace ADD HasLockedDecision BIT NOT NULL
GO

ALTER TABLE DecisionTrace DROP COLUMN TimestampCounter
GO

ALTER TABLE DecisionTrace ADD TimestampCounter ROWVERSION
GO
