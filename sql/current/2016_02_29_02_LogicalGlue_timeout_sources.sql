SET QUOTED_IDENTIFIER ON
GO

IF 0 = COLUMNPROPERTY(OBJECT_ID('LogicalGlueTimeoutSources'), 'TimeoutSourceID', 'IsIdentity')
BEGIN
	DECLARE @ErrorMsg NVARCHAR(1024) = NULL
	DECLARE @WasError BIT = 0

	BEGIN TRANSACTION

	BEGIN TRY
		ALTER TABLE LogicalGlueResponses DROP CONSTRAINT FK_LogicalGlueResponses_TimeoutSource

		SELECT
			TimeoutSourceID,
			TimeoutSource,
			CommunicationCode
		INTO
			oldLogicalGlueTimeoutSources
		FROM
			LogicalGlueTimeoutSources

		DROP TABLE LogicalGlueTimeoutSources

		CREATE TABLE LogicalGlueTimeoutSources (
			TimeoutSourceID BIGINT IDENTITY(1, 1) NOT NULL,
			TimeoutSource NVARCHAR(255) NOT NULL,
			CommunicationCode NVARCHAR(5) NOT NULL,
			TimestampCounter ROWVERSION,
			CONSTRAINT PK_LogicalGlueTimeoutSources PRIMARY KEY (TimeoutSourceID),
			CONSTRAINT UC_LogicalGlueTimeoutSources UNIQUE (TimeoutSource),
			CONSTRAINT CHK_LogicalGlueTimeoutSources CHECK (LTRIM(RTRIM(TimeoutSource)) != ''),
			CONSTRAINT UC_LogicalGlueTimeoutSources_CommCode UNIQUE (CommunicationCode),
			CONSTRAINT CHK_LogicalGlueTimeoutSources_CommCode CHECK (LTRIM(RTRIM(CommunicationCode)) != '')
		)

		SET IDENTITY_INSERT LogicalGlueTimeoutSources ON

		INSERT INTO LogicalGlueTimeoutSources(TimeoutSourceID, TimeoutSource, CommunicationCode)
		SELECT
			TimeoutSourceID,
			TimeoutSource,
			CommunicationCode
		FROM
			oldLogicalGlueTimeoutSources

		SET IDENTITY_INSERT LogicalGlueTimeoutSources OFF

		DROP TABLE oldLogicalGlueTimeoutSources

		ALTER TABLE LogicalGlueResponses ADD CONSTRAINT FK_LogicalGlueResponses_TimeoutSource FOREIGN KEY (TimeoutSourceID) REFERENCES LogicalGlueTimeoutSources (TimeoutSourceID)
	END TRY
	BEGIN CATCH
		SET @ErrorMsg = dbo.udfGetErrorMsg()
		SET @WasError = 1

		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION
	END CATCH

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION

	IF @ErrorMsg IS NOT NULL
		RAISERROR('Failed to convert TimeoutSourceID to IDENTITY: %s', 11, 0, @ErrorMsg)
	ELSE IF @WasError = 1
		RAISERROR('Failed to convert TimeoutSourceID to IDENTITY for some reason', 11, 0)
END
GO
