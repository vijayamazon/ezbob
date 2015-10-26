SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueRequestFeatureTypes') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequestFeatureTypes (
		FeatureTypeID BIGINT NOT NULL,
		FeatureType NVARCHAR(64) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequestFeatureTypes PRIMARY KEY (FeatureTypeID),
		CONSTRAINT UC_LogicalGlueRequestFeatureTypes UNIQUE (FeatureType),
		CONSTRAINT CHK_LogicalGlueRequestFeatureTypes CHECK (LTRIM(RTRIM(FeatureType)) != '')
	)

	INSERT INTO LogicalGlueRequestFeatureTypes (FeatureTypeID, FeatureType) VALUES
		(1, 'continuous'),
		(2, 'categorical'),
		(3, 'mixed')
END
GO

IF OBJECT_ID('LogicalGlueRequestSchema') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequestSchema (
		FeatureID BIGINT IDENTITY(1, 1) NOT NULL,
		IsActive BIT NOT NULL,
		InternalFeatureName NVARCHAR(255) NOT NULL,
		RawFeatureName NVARCHAR(255) NOT NULL,
		FeatureAlias NVARCHAR(255) NOT NULL,
		FeatureTypeID BIGINT NOT NULL,
		MinValue DECIMAL(32, 16) NULL,
		MaxValue DECIMAL(32, 16) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequestSchema PRIMARY KEY (FeatureID),
		CONSTRAINT FK_LogicalGlueRequestSchema_FeatureType FOREIGN KEY (FeatureTypeID) REFERENCES LogicalGlueRequestFeatureTypes (FeatureTypeID)
	)


	INSERT INTO LogicalGlueRequestSchema (IsActive, InternalFeatureName, RawFeatureName, FeatureAlias, FeatureTypeID, MinValue, MaxValue) SELECT 1, 'MODEL_UNIQUE_ID', 'modelUuid', 'Model unique ID', FeatureTypeID, NULL, NULL FROM LogicalGlueRequestFeatureTypes WHERE FeatureType = 'continuous'
	INSERT INTO LogicalGlueRequestSchema (IsActive, InternalFeatureName, RawFeatureName, FeatureAlias, FeatureTypeID, MinValue, MaxValue) SELECT 1, 'MODEL_PASSWORD_ID', 'modelPasswordUuid', 'Model password ID', FeatureTypeID, NULL, NULL FROM LogicalGlueRequestFeatureTypes WHERE FeatureType = 'continuous'
END
GO

IF OBJECT_ID('LogicalGlueRequestFeatureCategories') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequestFeatureCategories (
		CategoryID BIGINT IDENTITY(1, 1) NOT NULL,
		FeatureID BIGINT NOT NULL,
		Category NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequestFeatureCategories PRIMARY KEY (CategoryID),
		CONSTRAINT FK_LogicalGlueRequestFeatureCategories_Feature FOREIGN KEY (FeatureID) REFERENCES LogicalGlueRequestSchema (FeatureID)
	)

END
GO

IF OBJECT_ID('LogicalGlueRequestItemValueTypes') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequestItemValueTypes (
		ValueTypeID BIGINT IDENTITY(1, 1) NOT NULL,
		ValueType NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequestItemValueTypes PRIMARY KEY (ValueTypeID)
	)
END
GO

IF OBJECT_ID('LogicalGlueRequestTypes') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequestTypes (
		RequestTypeID BIGINT NOT NULL,
		InternalRequestType NVARCHAR(255) NOT NULL,
		RequestType NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequestTypes PRIMARY KEY (RequestTypeID),
		CONSTRAINT UC_LogicalGlueRequestTypes_Internal UNIQUE (InternalRequestType),
		CONSTRAINT UC_LogicalGlueRequestTypes_External UNIQUE (RequestType),
		CONSTRAINT CHK_LogicalGlueRequestTypes CHECK (
			LTRIM(RTRIM(InternalRequestType)) != ''
			AND
			LTRIM(RTRIM(RequestType)) != ''
		)
	)

	INSERT INTO LogicalGlueRequestTypes (RequestTypeID, InternalRequestType, RequestType) VALUES
		(1, 'LoGlueFuzzyLogic', 'Fuzzy logic'),
		(2, 'LoGlueNeuralNetwork', 'Neural network')
END
GO

IF OBJECT_ID('LogicalGlueRequests') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequests (
		RequestID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		ServiceLogID BIGINT NULL,
		RequestTypeID BIGINT NOT NULL,
		SendingTime DATETIME NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequests PRIMARY KEY (RequestID),
		CONSTRAINT FK_LogicalGlueRequests_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
		CONSTRAINT FK_LogicalGlueRequests_SericeLog FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog (Id),
		CONSTRAINT FK_LogicalGlueRequests_Type FOREIGN KEY (RequestTypeID) REFERENCES LogicalGlueRequestTypes (RequestTypeID)
	)
END
GO

IF OBJECT_ID('LogcialGlueRequestItems') IS NULL
BEGIN
	CREATE TABLE LogcialGlueRequestItems (
		RequestItemID BIGINT IDENTITY(1, 1) NOT NULL,
		RequestID BIGINT NOT NULL,
		FeatureID BIGINT NULL, -- TODO: make not nullable when schema is stable
		Name NVARCHAR(255) NOT NULL, -- TODO: drop it when schema is stable
		ValueTypeID BIGINT NOT NULL,
		Value NVARCHAR(MAX) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogcialGlueRequestItems PRIMARY KEY (RequestItemID),
		CONSTRAINT FK_LogcialGlueRequestItems_Request FOREIGN KEY (RequestID) REFERENCES LogicalGlueRequests (RequestID),
		CONSTRAINT FK_LogcialGlueRequestItems_RequestSchema FOREIGN KEY (FeatureID) REFERENCES LogicalGlueRequestSchema (FeatureID),
		CONSTRAINT FK_LogcialGlueRequestItems_ValueType FOREIGN KEY (ValueTypeID) REFERENCES LogicalGlueRequestItemValueTypes (ValueTypeID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponses') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponses (
		ResponseID BIGINT IDENTITY(1, 1) NOT NULL,
		RequestID BIGINT NOT NULL,
		ReceivingTime DATETIME NOT NULL,
		InferenceResultEncoded BIGINT NULL,
		InferenceResultDecoded NVARCHAR(255) NULL,
		Score DECIMAL(18, 16) NULL,
		Status NVARCHAR(255) NOT NULL,
		Exception NVARCHAR(MAX) NULL,
		ErrorCode NVARCHAR(MAX) NULL,
		Uuid UNIQUEIDENTIFIER NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogcialGlueResponses PRIMARY KEY (ResponseID),
		CONSTRAINT FK_LogcialGlueResponses_Request FOREIGN KEY (RequestID) REFERENCES LogicalGlueRequests (RequestID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponseMapOutputRatios') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponseMapOutputRatios (
		OutputRatioID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		OutputClass NVARCHAR(255) NOT NULL,
		Score DECIMAL(18, 16),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogcialGlueResponseMapOutputRatios PRIMARY KEY (OutputRatioID),
		CONSTRAINT FK_LogcialGlueResponseMapOutputRatios_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponseWarnings') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponseWarnings (
		WarningID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		FeatureID BIGINT NULL,
		Value NVARCHAR(MAX) NULL,
		FeatureName NVARCHAR(255) NULL,
		MinValue NVARCHAR(255) NULL,
		MaxValue NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponseWarnings PRIMARY KEY (WarningID),
		CONSTRAINT FK_LogicalGlueResponseWarnings_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID),
		CONSTRAINT FK_LogicalGlueResponseWarnings_RequestSchema FOREIGN KEY (FeatureID) REFERENCES LogicalGlueRequestSchema (FeatureID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponseEncodingFailures') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponseEncodingFailures (
		FailureID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		FeatureID BIGINT NULL,
		ColumnName NVARCHAR(255) NOT NULL,
		UnencodedValue NVARCHAR(MAX) NULL,
		Reason NVARCHAR(MAX) NULL,
		Message NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponseEncodingFailures PRIMARY KEY (FailureID),
		CONSTRAINT FK_LogicalGlueResponseEncodingFailures_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID),
		CONSTRAINT FK_LogicalGlueResponseEncodingFailures_RequestSchema FOREIGN KEY (FeatureID) REFERENCES LogicalGlueRequestSchema (FeatureID)
	)
END
GO
