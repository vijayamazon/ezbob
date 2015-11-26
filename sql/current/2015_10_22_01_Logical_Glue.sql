SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueRequests') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequests (
		RequestID BIGINT IDENTITY(1, 1) NOT NULL,
		ServiceLogID BIGINT NOT NULL,
		UniqueID UNIQUEIDENTIFIER NOT NULL,
		MonthlyRepayment DECIMAL(18, 0) NOT NULL,
		CompanyRegistrationNumber NVARCHAR(32) NULL,
		FirstName NVARCHAR(250) NULL,
		LastName NVARCHAR(250) NULL,
		DateOfBirth DATETIME NULL,
		EquifaxData NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequests PRIMARY KEY (RequestID),
		CONSTRAINT FK_LogcialGlueRequests_ServiceLog FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog (Id)
	)
END
GO

IF OBJECT_ID('LogicalGlueRequestTypes') IS NULL
BEGIN
	CREATE TABLE LogicalGlueRequestTypes (
		RequestTypeID BIGINT NOT NULL,
		RequestType NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueRequestTypes PRIMARY KEY (RequestTypeID),
		CONSTRAINT UC_LogicalGlueRequestTypes_External UNIQUE (RequestType),
		CONSTRAINT CHK_LogicalGlueRequestTypes CHECK (LTRIM(RTRIM(RequestType)) != '')
	)

	INSERT INTO LogicalGlueRequestTypes (RequestTypeID, RequestType) VALUES
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
		BucketID BIGINT NULL,
		HasEquifaxData BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogicalGlueResponses PRIMARY KEY (ResponseID),
		CONSTRAINT FK_LogcialGlueResponses_ServiceLog FOREIGN KEY (ServiceLogID) REFERENCES MP_ServiceLog (Id),
		CONSTRAINT FK_LogcialGlueResponses_Bucket FOREIGN KEY (BucketID) REFERENCES LogicalGlueBuckets (BucketID)
	)
END
GO

IF OBJECT_ID('LogicalGlueModelOutputs') IS NULL
BEGIN
	CREATE TABLE LogicalGlueModelOutputs (
		ModelOutputID BIGINT IDENTITY(1, 1) NOT NULL,
		ResponseID BIGINT NOT NULL,
		RequestTypeID BIGINT NOT NULL,
		InferenceResultEncoded BIGINT NULL,
		InferenceResultDecoded NVARCHAR(255) NULL,
		Score DECIMAL(18, 16) NULL,
		Status NVARCHAR(255) NOT NULL,
		Exception NVARCHAR(MAX) NULL,
		ErrorCode NVARCHAR(MAX) NULL,
		Uuid UNIQUEIDENTIFIER NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogcialGlueModelOutputs PRIMARY KEY (ModelOutputID),
		CONSTRAINT FK_LogcialGlueModelOutputs_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID),
		CONSTRAINT FK_LogcialGlueModelOutputs_RequestType FOREIGN KEY (RequestTypeID) REFERENCES LogicalGlueRequestTypes (RequestTypeID)
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
		CONSTRAINT PK_LogcialGlueModelOutputRatios PRIMARY KEY (OutputRatioID),
		CONSTRAINT FK_LogcialGlueModelOutputRatios_ModelOutput FOREIGN KEY (ModelOutputID) REFERENCES LogicalGlueModelOutputs (ModelOutputID)
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
