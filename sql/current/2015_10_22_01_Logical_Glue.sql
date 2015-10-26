SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
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

IF OBJECT_ID('LogicalGlueResponses') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponses (
		ResponseID BIGINT IDENTITY(1, 1) NOT NULL,
		ServiceLogID BIGINT NOT NULL,
		ReceivingTime DATETIME NOT NULL,
		RequestTypeID BIGINT NOT NULL,
		InferenceResultEncoded BIGINT NULL,
		InferenceResultDecoded NVARCHAR(255) NULL,
		Score DECIMAL(18, 16) NULL,
		Status NVARCHAR(255) NOT NULL,
		Exception NVARCHAR(MAX) NULL,
		ErrorCode NVARCHAR(MAX) NULL,
		Uuid UNIQUEIDENTIFIER NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogcialGlueResponses PRIMARY KEY (ResponseID),
		CONSTRAINT FK_LogcialGlueResponses_ServiceLog FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog (Id),
		CONSTRAINT FK_LogcialGlueResponses_RequestType FOREIGN KEY (RequestTypeID) REFERENCES LogicalGlueRequestTypes (RequestTypeID)
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
		Value NVARCHAR(MAX) NULL,
		FeatureName NVARCHAR(255) NULL,
		MinValue NVARCHAR(255) NULL,
		MaxValue NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponseWarnings PRIMARY KEY (WarningID),
		CONSTRAINT FK_LogicalGlueResponseWarnings_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponseEncodingFailures') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponseEncodingFailures (
		FailureID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		RowIndex INT NOT NULL,
		ColumnName NVARCHAR(255) NOT NULL,
		UnencodedValue NVARCHAR(MAX) NULL,
		Reason NVARCHAR(MAX) NULL,
		Message NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponseEncodingFailures PRIMARY KEY (FailureID),
		CONSTRAINT FK_LogicalGlueResponseEncodingFailures_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponseListRangeErrors') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponseListRangeErrors (
		ListRangeErrorID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		ColumnName NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponseListRangeErrors PRIMARY KEY (ListRangeErrorID),
		CONSTRAINT FK_LogicalGlueResponseListRangeErrors_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponseMissingColumns') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponseMissingColumns (
		MissingColumnID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		ColumnName NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponseMissingColumns PRIMARY KEY (MissingColumnID),
		CONSTRAINT FK_LogicalGlueResponseMissingColumns_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID)
	)
END
GO
