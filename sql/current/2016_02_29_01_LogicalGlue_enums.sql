SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------
--
-- LogicalGlueTimeoutSources.CommunicationCode
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LogicalGlueTimeoutSources') AND name = 'CommunicationCode')
BEGIN
	ALTER TABLE LogicalGlueTimeoutSources DROP COLUMN TimestampCounter

	ALTER TABLE LogicalGlueTimeoutSources ADD CommunicationCode NVARCHAR(5) NULL

	EXECUTE('UPDATE LogicalGlueTimeoutSources SET CommunicationCode = ''E'' WHERE TimeoutSource = ''Equifax system'' ')
	EXECUTE('UPDATE LogicalGlueTimeoutSources SET CommunicationCode = ''L'' WHERE TimeoutSource = ''Logical Glue Inference API'' ')

	EXECUTE('ALTER TABLE LogicalGlueTimeoutSources ALTER COLUMN CommunicationCode NVARCHAR(5) NOT NULL')

	ALTER TABLE LogicalGlueTimeoutSources ADD CONSTRAINT UC_LogicalGlueTimeoutSources_CommCode UNIQUE (CommunicationCode)

	ALTER TABLE LogicalGlueTimeoutSources ADD TimestampCounter ROWVERSION
END
GO

-------------------------------------------------------------------------------
--
-- LogicalGlueEtlCodes.IsError
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LogicalGlueEtlCodes') AND name = 'IsError')
BEGIN
	ALTER TABLE LogicalGlueEtlCodes DROP COLUMN TimestampCounter

	ALTER TABLE LogicalGlueEtlCodes ADD IsError BIT NULL

	EXECUTE('UPDATE LogicalGlueEtlCodes SET IsError = 0')

	EXECUTE('ALTER TABLE LogicalGlueEtlCodes ALTER COLUMN IsError BIT NOT NULL')

	ALTER TABLE LogicalGlueEtlCodes ADD TimestampCounter ROWVERSION
END
GO

-------------------------------------------------------------------------------
--
-- LogicalGlueEtlCodes.CommunicationCode
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LogicalGlueEtlCodes') AND name = 'CommunicationCode')
BEGIN
	ALTER TABLE LogicalGlueEtlCodes DROP COLUMN TimestampCounter

	ALTER TABLE LogicalGlueEtlCodes ADD CommunicationCode NVARCHAR(5) NULL

	EXECUTE('UPDATE LogicalGlueEtlCodes SET CommunicationCode = ''P'' WHERE EtlCode = ''Success'' ')
	EXECUTE('UPDATE LogicalGlueEtlCodes SET CommunicationCode = ''R'' WHERE EtlCode = ''Hard reject'' ')

	EXECUTE('ALTER TABLE LogicalGlueEtlCodes ALTER COLUMN CommunicationCode NVARCHAR(5) NOT NULL')

	ALTER TABLE LogicalGlueEtlCodes ADD CONSTRAINT UC_LogicalGlueEtlCodes_CommCode UNIQUE (CommunicationCode)

	ALTER TABLE LogicalGlueEtlCodes ADD TimestampCounter ROWVERSION
END
GO
