SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
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

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueHostName')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueHostName',
		'etl.live.logicalglue.net',
		'String. Host name of the Logical Glue API URL.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueNewCustomerRequestPath')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueNewCustomerRequestPath',
		'/everline-scores-with-report/{0}',
		'String. Full path to resource of Logical Glue API URL for new customer.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'LogicalGlueOldCustomerRequestPath')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'LogicalGlueOldCustomerRequestPath',
		'/everline-scores/{0}',
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
		UniqueID UNIQUEIDENTIFIER NOT NULL,
		MonthlyRepayment DECIMAL(18, 0) NULL,
		EquifaxData NVARCHAR(MAX) NULL,
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

IF OBJECT_ID('LogicalGlueBuckets') IS NULL
BEGIN
	CREATE TABLE LogicalGlueBuckets (
		BucketID BIGINT NOT NULL,
		Bucket NCHAR(1) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueBuckets PRIMARY KEY (BucketID),
		CONSTRAINT UC_LogicalGlueBuckets UNIQUE (Bucket)
	)

	INSERT INTO LogicalGlueBuckets (BucketID, Bucket) VALUES
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

IF OBJECT_ID('LogicalGlueResponses') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponses (
		ResponseID BIGINT IDENTITY(1, 1) NOT NULL,
		ServiceLogID BIGINT NOT NULL,
		ReceivedTime DATETIME NOT NULL,
		HttpStatus INT NOT NULL,
		BucketID BIGINT NULL,
		HasEquifaxData BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponses PRIMARY KEY (ResponseID),
		CONSTRAINT FK_LogicalGlueResponses_ServiceLog FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog (Id),
		CONSTRAINT FK_LogicalGlueResponses_Bucket FOREIGN KEY (BucketID) REFERENCES LogicalGlueBuckets (BucketID)
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
