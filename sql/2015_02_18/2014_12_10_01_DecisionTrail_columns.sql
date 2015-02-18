SET QUOTED_IDENTIFIER ON
GO

DECLARE @Changed BIT = 0

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'CashRequestID')
BEGIN
	SET @Changed = 1

	ALTER TABLE DecisionTrail ADD CashRequestID BIGINT NULL

	ALTER TABLE DecisionTrail ADD CONSTRAINT FK_DecisionTrail_CashRequest FOREIGN KEY (CashRequestID) REFERENCES CashRequests (Id)
END

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'Tag')
BEGIN
	SET @Changed = 1

	ALTER TABLE DecisionTrail ADD Tag NVARCHAR(256) NULL
END

IF @Changed = 1
BEGIN
	ALTER TABLE DecisionTrail DROP COLUMN TimestampCounter

	ALTER TABLE DecisionTrail ADD TimestampCounter ROWVERSION
END

GO
