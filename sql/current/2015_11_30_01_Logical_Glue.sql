SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueAuthorizationScheme')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueAuthorizationScheme',
		'Basic',
		'String. Authorization scheme used to make Logical Glue requests.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueUserName')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueUserName',
		'everline',
		'String. User name used to make Logical Glue requests.',
		0
	)
END
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGluePassword')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGluePassword',
		CASE @Environment WHEN 'Prod' THEN 'At5quijohghu' ELSE 'nexahbaey8ei' END,
		'String. Password used to make Logical Glue requests.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueCacheAcceptanceDays')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueCacheAcceptanceDays',
		'30',
		'Integer. Logical Glue inference is valid for this number of days; it should be updated if older.',
		0
	)
END
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueHostName')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueHostName',
		CASE @Environment WHEN 'Prod' THEN 'etl.live.logicalglue.net' ELSE 'etl-dev.live.logicalglue.net' END,
		'String. Host name of the Logical Glue API URL.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueNewCustomerRequestPath')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueNewCustomerRequestPath',
		'/equifax-scores-with-report/{0}',
		'String. Full path to resource of Logical Glue API URL for new customer.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueOldCustomerRequestPath')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueOldCustomerRequestPath',
		'/equifax-scores/{0}',
		'String. Full path to resource of Logical Glue API URL for returning customer.',
		0
	)
END
GO

IF OBJECT_ID('LogicalGlueRequests') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequests (
		RequestID BIGINT IDENTITY(1, 1) NOT NULL,
		ServiceLogID BIGINT NOT NULL,
		IsTryOut BIT NOT NULL,
		UniqueID UNIQUEIDENTIFIER NOT NULL,
		MonthlyRepayment DECIMAL(18, 0) NULL,
		EquifaxData NVARCHAR(MAX) NULL,
		HouseName NVARCHAR(255) NULL,
		HouseNumber NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequests PRIMARY KEY (RequestID),
		CONSTRAINT FK_LogicalGlueRequests_ServiceLog FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog (Id)
	)
END
GO

IF OBJECT_ID('LogicalGlueModels') IS NULL
BEGIN
	CREATE TABLE LogicalGlueModels (
		ModelID BIGINT NOT NULL,
		ModelName NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueModels PRIMARY KEY (ModelID),
		CONSTRAINT UC_LogicalGlueModels UNIQUE (ModelName),
		CONSTRAINT CHK_LogicalGlueModels CHECK (LTRIM(RTRIM(ModelName)) != '')
	)

	INSERT INTO LogicalGlueModels (ModelID, ModelName) VALUES
		(1, 'Fuzzy logic'),
		(2, 'Neural network')
END
GO

IF OBJECT_ID('I_Grade') IS NULL
BEGIN
	CREATE TABLE I_Grade (
		GradeID INT NOT NULL,
		Name NVARCHAR(5) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_I_Grade PRIMARY KEY (GradeID),
		CONSTRAINT UC_I_Grade UNIQUE (Name),
		CONSTRAINT CHK_I_Grade CHECK (LTRIM(RTRIM(Name)) != '')
	)

	INSERT INTO I_Grade (GradeID, Name) VALUES
		(1, 'A'),
		(2, 'B'),
		(3, 'C'),
		(4, 'D'),
		(5, 'E'),
		(6, 'F'),
		(7, 'G'),
		(8, 'H')
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'UpperBound' AND id = OBJECT_ID('I_Grade'))
BEGIN
	ALTER TABLE I_Grade DROP COLUMN TimestampCounter

	ALTER TABLE I_Grade ADD UpperBound DECIMAL(18, 6) NULL

	EXECUTE('
		UPDATE I_Grade SET UpperBound = 0.155 WHERE Name = ''A''
		UPDATE I_Grade SET UpperBound = 0.253 WHERE Name = ''B''
		UPDATE I_Grade SET UpperBound = 0.347 WHERE Name = ''C''
		UPDATE I_Grade SET UpperBound = 0.452 WHERE Name = ''D''
		UPDATE I_Grade SET UpperBound = 0.552 WHERE Name = ''E''
		UPDATE I_Grade SET UpperBound = 0.594 WHERE Name = ''F''
		UPDATE I_Grade SET UpperBound = 0.697 WHERE Name = ''G''
		UPDATE I_Grade SET UpperBound = 1.000 WHERE Name = ''H''

		ALTER TABLE I_Grade ALTER COLUMN UpperBound DECIMAL(18, 6) NOT NULL
	')

	ALTER TABLE I_Grade ADD TimestampCounter ROWVERSION
END
GO

IF OBJECT_ID('LogicalGlueTimeoutSources') IS NULL
BEGIN
	CREATE TABLE LogicalGlueTimeoutSources (
		TimeoutSourceID BIGINT NOT NULL,
		TimeoutSource NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueTimeoutSources PRIMARY KEY (TimeoutSourceID),
		CONSTRAINT UC_LogicalGlueTimeoutSources UNIQUE (TimeoutSource),
		CONSTRAINT CHK_LogicalGlueTimeoutSources CHECK (LTRIM(RTRIM(TimeoutSource)) != '')
	)

	INSERT INTO LogicalGlueTimeoutSources (TimeoutSourceID, TimeoutSource) VALUES
		(1, 'Equifax system'),
		(2, 'Logical Glue Inference API')
END
GO

IF OBJECT_ID('LogicalGlueEtlCodes') IS NULL
BEGIN
	CREATE TABLE LogicalGlueEtlCodes (
		EtlCodeID BIGINT NOT NULL,
		EtlCode NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueEtlCodes PRIMARY KEY (EtlCodeID),
		CONSTRAINT UC_LogicalGlueEtlCodes UNIQUE (EtlCode),
		CONSTRAINT CHK_LogicalGlueEtlCodes CHECK (LTRIM(RTRIM(EtlCode)) != '')
	)

	INSERT INTO LogicalGlueEtlCodes (EtlCodeID, EtlCode) VALUES
		(1, 'Success'),
		(2, 'Hard reject')
END
GO

IF OBJECT_ID('LogicalGlueResponses') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponses (
		ResponseID BIGINT IDENTITY(1, 1) NOT NULL,
		ServiceLogID BIGINT NOT NULL,
		ReceivedTime DATETIME NOT NULL,
		HttpStatus INT NOT NULL, -- actual status of the actual REST request.
		ResponseStatus INT NOT NULL, -- status received from LG in the field 'status' of the root object.
		TimeoutSourceID BIGINT NULL,
		ErrorMessage NVARCHAR(MAX) NULL,
		GradeID INT NULL,
		HasEquifaxData BIT NOT NULL,
		ParsingExceptionType NVARCHAR(MAX) NULL,
		ParsingExceptionMessage NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponses PRIMARY KEY (ResponseID),
		CONSTRAINT FK_LogicalGlueResponses_ServiceLog FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog (Id),
		CONSTRAINT FK_LogicalGlueResponses_Grade FOREIGN KEY (GradeID) REFERENCES I_Grade (GradeID),
		CONSTRAINT FK_LogicalGlueResponses_TimeoutSource FOREIGN KEY (TimeoutSourceID) REFERENCES LogicalGlueTimeoutSources (TimeoutSourceID)
	)
END
GO

IF OBJECT_ID('LogicalGlueEtlData') IS NULL
BEGIN
	CREATE TABLE LogicalGlueEtlData (
		EtlDataID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		EtlCodeID BIGINT NULL,
		Message NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueEtlData PRIMARY KEY (EtlDataID),
		CONSTRAINT FK_LogicalGlueEtlData_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID),
		CONSTRAINT FK_LogicalGlueEtlData_Code FOREIGN KEY (EtlCodeID) REFERENCES LogicalGlueEtlCodes (EtlCodeID)
	)
END
GO

IF OBJECT_ID('LogicalGlueModelOutputs') IS NULL
BEGIN
	CREATE TABLE LogicalGlueModelOutputs (
		ModelOutputID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		ModelID BIGINT NOT NULL,
		InferenceResultEncoded BIGINT NULL,
		InferenceResultDecoded NVARCHAR(255) NULL,
		Score DECIMAL(18, 16) NULL,
		Status NVARCHAR(255) NOT NULL,
		Exception NVARCHAR(MAX) NULL,
		ErrorCode NVARCHAR(MAX) NULL,
		Uuid UNIQUEIDENTIFIER NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueModelOutputs PRIMARY KEY (ModelOutputID),
		CONSTRAINT FK_LogicalGlueModelOutputs_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID),
		CONSTRAINT FK_LogicalGlueModelOutputs_Model FOREIGN KEY (ModelID) REFERENCES LogicalGlueModels (ModelID)
	)
END
GO

IF OBJECT_ID('LogicalGlueModelOutputRatios') IS NULL
BEGIN
	CREATE TABLE LogicalGlueModelOutputRatios (
		OutputRatioID BIGINT IDENTITY(1, 1) NOT NULL,
		ModelOutputID BIGINT NOT NULL,
		OutputClass NVARCHAR(255) NOT NULL,
		Score DECIMAL(18, 16) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueModelOutputRatios PRIMARY KEY (OutputRatioID),
		CONSTRAINT FK_LogicalGlueModelOutputRatios_ModelOutput FOREIGN KEY (ModelOutputID) REFERENCES LogicalGlueModelOutputs (ModelOutputID)
	)
END
GO

IF OBJECT_ID('LogicalGlueModelWarnings') IS NULL
BEGIN
	CREATE TABLE LogicalGlueModelWarnings (
		WarningID BIGINT IDENTITY(1, 1) NOT NULL,
		ModelOutputID BIGINT NOT NULL,
		Value NVARCHAR(MAX) NULL,
		FeatureName NVARCHAR(255) NULL,
		MinValue NVARCHAR(255) NULL,
		MaxValue NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueModelWarnings PRIMARY KEY (WarningID),
		CONSTRAINT FK_LogicalGlueModelWarnings_ModelOutput FOREIGN KEY (ModelOutputID) REFERENCES LogicalGlueModelOutputs (ModelOutputID)
	)
END
GO

IF OBJECT_ID('LogicalGlueModelEncodingFailures') IS NULL
BEGIN
	CREATE TABLE LogicalGlueModelEncodingFailures (
		FailureID BIGINT IDENTITY(1, 1) NOT NULL,
		ModelOutputID BIGINT NOT NULL,
		RowIndex INT NOT NULL,
		ColumnName NVARCHAR(255) NOT NULL,
		UnencodedValue NVARCHAR(MAX) NULL,
		Reason NVARCHAR(MAX) NULL,
		Message NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueModelEncodingFailures PRIMARY KEY (FailureID),
		CONSTRAINT FK_LogicalGlueModelEncodingFailures_ModelOutput FOREIGN KEY (ModelOutputID) REFERENCES LogicalGlueModelOutputs (ModelOutputID)
	)
END
GO

IF OBJECT_ID('LogicalGlueModelMissingColumns') IS NULL
BEGIN
	CREATE TABLE LogicalGlueModelMissingColumns (
		MissingColumnID BIGINT IDENTITY(1, 1) NOT NULL,
		ModelOutputID BIGINT NOT NULL,
		ColumnName NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueModelMissingColumns PRIMARY KEY (MissingColumnID),
		CONSTRAINT FK_LogicalGlueModelMissingColumns_ModelOutput FOREIGN KEY (ModelOutputID) REFERENCES LogicalGlueModelOutputs (ModelOutputID)
	)
END
GO
