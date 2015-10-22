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
	)
END
GO
