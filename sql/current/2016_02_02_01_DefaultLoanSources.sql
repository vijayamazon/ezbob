SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('DefaultLoanSources') IS NULL
BEGIN
	CREATE TABLE DefaultLoanSources (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanSourceID INT NOT NULL,
		OriginID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_DefaultLoanSources PRIMARY KEY(EntryID),
		CONSTRAINT UC_DefaultLoanSources UNIQUE(LoanSourceID, OriginID),
		CONSTRAINT FK_DefaultLoanSources_Source FOREIGN KEY (LoanSourceID) REFERENCES LoanSource(LoanSourceID),
		CONSTRAINT FK_DefaultLoanSources_Origin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID)
	)

	INSERT INTO DefaultLoanSources(LoanSourceID, OriginID)
	SELECT
		1,
		CustomerOriginID
	FROM
		CustomerOrigin
END
GO

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanSource') AND name = 'IsDefault')
BEGIN
	ALTER TABLE LoanSource DROP CONSTRAINT DF_LoanSource_Default
	ALTER TABLE LoanSource DROP COLUMN IsDefault
END
GO
