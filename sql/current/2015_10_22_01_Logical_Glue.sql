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
		MonthlyRepayment DECIMAL(18, 0) NOT NULL,
		ReceivingTime DATETIME NOT NULL,
		RequestTypeID BIGINT NOT NULL,
		BucketID BIGINT NULL,
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
		CONSTRAINT FK_LogcialGlueResponses_RequestType FOREIGN KEY (RequestTypeID) REFERENCES LogicalGlueRequestTypes (RequestTypeID),
		CONSTRAINT FK_LogcialGlueResponses_Bucket FOREIGN KEY (BucketID) REFERENCES LogicalGlueBuckets (BucketID)
	)
END
GO

IF OBJECT_ID('LogicalGlueResponseOutputRatios') IS NULL
BEGIN
	CREATE TABLE LogicalGlueResponseOutputRatios (
		OutputRatioID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		OutputClass NVARCHAR(255) NOT NULL,
		Score DECIMAL(18, 16) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogcialGlueResponseOutputRatios PRIMARY KEY (OutputRatioID),
		CONSTRAINT FK_LogcialGlueResponseOutputRatios_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID)
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
