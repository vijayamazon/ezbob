SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('Matrices') IS NULL
BEGIN
	CREATE TABLE Matrices (
		MatrixID BIGINT IDENTITY(1, 1) NOT NULL,
		MatrixName NVARCHAR(64) NOT NULL,
		Description NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Matrices PRIMARY KEY (MatrixID),
		CONSTRAINT UC_Matrices UNIQUE (MatrixName),
		CONSTRAINT CHK_Matrices CHECK (LTRIM(RTRIM(MatrixName)) != '')
	)
END
GO

IF OBJECT_ID('MatrixRowTitles') IS NULL
BEGIN
	CREATE TABLE MatrixRowTitles (
		MatrixRowID BIGINT IDENTITY(1, 1) NOT NULL,
		MatrixID BIGINT NOT NULL,
		TitleValue DECIMAL(18, 6) NULL,
		PositiveInfinity BIT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_MatrixRowTitles PRIMARY KEY (MatrixRowID),
		CONSTRAINT FK_MatrixRowTitles_Matrix FOREIGN KEY (MatrixID) REFERENCES Matrices (MatrixID),
		CONSTRAINT CHK_MatrixRowTitles CHECK (
			(TitleValue IS NOT NULL AND PositiveInfinity IS NULL)
			OR
			(TitleValue IS NULL AND PositiveInfinity IS NOT NULL)
		),
		CONSTRAINT UC_MatrixRowTitles UNIQUE (MatrixID, TitleValue, PositiveInfinity)
	)
END
GO

IF OBJECT_ID('MatrixColumns') IS NULL
BEGIN
	CREATE TABLE MatrixColumns (
		MatrixColumnID BIGINT IDENTITY(1, 1) NOT NULL,
		MatrixRowID BIGINT NOT NULL,
		TitleValue DECIMAL(18, 6) NULL,
		PositiveInfinity BIT NULL,
		CellValue DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_MatrixColumns PRIMARY KEY (MatrixColumnID),
		CONSTRAINT FK_MatrixColumns_MatrixRow FOREIGN KEY (MatrixRowID) REFERENCES MatrixRowTitles (MatrixRowID),
		CONSTRAINT CHK_MatrixColumns CHECK (
			(TitleValue IS NOT NULL AND PositiveInfinity IS NULL)
			OR
			(TitleValue IS NULL AND PositiveInfinity IS NOT NULL)
		),
		CONSTRAINT UC_MatrixColumns UNIQUE (MatrixRowID, TitleValue, PositiveInfinity)
	)
END
GO
