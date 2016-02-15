SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF OBJECT_ID('CustomerLogicalGlueHistory') IS NULL
BEGIN
	CREATE TABLE CustomerLogicalGlueHistory (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		CompanyID INT NOT NULL,
		ResponseID BIGINT NULL,
		IsActive BIT NOT NULL,
		GradeID INT NULL,
		Score DECIMAL(18, 6) NULL,
		IsHardReject BIT NOT NULL,
		ScoreIsReliable BIT NOT NULL,
		ErrorInResponse BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerLogicalGlueHistory PRIMARY KEY (EntryID),
		CONSTRAINT FK_CustomerLogicalGlueHistory_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
		CONSTRAINT FK_CustomerLogicalGlueHistory_Company FOREIGN KEY (CompanyID) REFERENCES Company (Id),
		CONSTRAINT FK_CustomerLogicalGlueHistory_Response FOREIGN KEY (ResponseID) REFERENCES LogicalGlueResponses (ResponseID),
		CONSTRAINT FK_CustomerLogicalGlueHistory_Grade FOREIGN KEY (GradeID) REFERENCES I_Grade (GradeID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LogicalGlueEtlCodes') AND name = 'IsHardReject')
BEGIN
	ALTER TABLE LogicalGlueEtlCodes DROP COLUMN TimestampCounter

	EXECUTE('ALTER TABLE LogicalGlueEtlCodes ADD IsHardReject BIT NULL')

	EXECUTE('UPDATE LogicalGlueEtlCodes SET IsHardReject = 0')
	EXECUTE('UPDATE LogicalGlueEtlCodes SET IsHardReject = 1 WHERE EtlCode = ''Hard reject''')

	EXECUTE('ALTER TABLE LogicalGlueEtlCodes ALTER COLUMN IsHardReject BIT NOT NULL')

	ALTER TABLE LogicalGlueEtlCodes ADD TimestampCounter ROWVERSION
END
GO
