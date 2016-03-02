SET QUOTED_IDENTIFIER ON
GO

IF 0 = COLUMNPROPERTY(OBJECT_ID('LogicalGlueEtlCodes'), 'EtlCodeID', 'IsIdentity')
BEGIN
	DECLARE @ErrorMsg NVARCHAR(1024) = NULL
	DECLARE @WasError BIT = 0

	BEGIN TRANSACTION

	BEGIN TRY
		ALTER TABLE LogicalGlueEtlData DROP CONSTRAINT FK_LogicalGlueEtlData_Code

		SELECT
			EtlCodeID,
			EtlCode,
			IsHardReject,
			IsError,
			CommunicationCode
		INTO
			oldLogicalGlueEtlCodes
		FROM
			LogicalGlueEtlCodes

		DROP TABLE LogicalGlueEtlCodes

		CREATE TABLE LogicalGlueEtlCodes (
			EtlCodeID BIGINT IDENTITY(1, 1) NOT NULL,
			EtlCode NVARCHAR(255) NOT NULL,
			IsHardReject BIT NOT NULL,
			IsError BIT NOT NULL,
			CommunicationCode NVARCHAR(5) NOT NULL,
			TimestampCounter ROWVERSION,
			CONSTRAINT PK_LogicalGlueEtlCodes PRIMARY KEY (EtlCodeID),
			CONSTRAINT UC_LogicalGlueEtlCodes UNIQUE (EtlCode),
			CONSTRAINT CHK_LogicalGlueEtlCodes CHECK (LTRIM(RTRIM(EtlCode)) != ''),
			CONSTRAINT UC_LogicalGlueEtlCodes_CommCode UNIQUE (CommunicationCode),
			CONSTRAINT CHK_LogicalGlueEtlCodes_CommCode CHECK (LTRIM(RTRIM(CommunicationCode)) != '')
		)

		SET IDENTITY_INSERT LogicalGlueEtlCodes ON

		INSERT INTO LogicalGlueEtlCodes(EtlCodeID, EtlCode, IsHardReject, IsError, CommunicationCode)
		SELECT
			EtlCodeID,
			EtlCode,
			IsHardReject,
			IsError,
			CommunicationCode
		FROM
			oldLogicalGlueEtlCodes

		SET IDENTITY_INSERT LogicalGlueEtlCodes OFF

		DROP TABLE oldLogicalGlueEtlCodes

		ALTER TABLE LogicalGlueEtlData ADD CONSTRAINT FK_LogicalGlueEtlData_Code FOREIGN KEY (EtlCodeID) REFERENCES LogicalGlueEtlCodes (EtlCodeID)
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
		RAISERROR('Failed to convert EtlCodeID to IDENTITY: %s', 11, 0, @ErrorMsg)
	ELSE IF @WasError = 1
		RAISERROR('Failed to convert EtlCodeID to IDENTITY for some reason', 11, 0)
END
GO
