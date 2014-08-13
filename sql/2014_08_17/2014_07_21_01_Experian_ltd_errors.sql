SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdErrors') IS NULL
BEGIN
	CREATE TABLE ExperianLtdErrors (
		ExperianLtdErrorsID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		ErrorMessage NTEXT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdErrors PRIMARY KEY (ExperianLtdErrorsID),
		CONSTRAINT FK_ExperianLtdErrors_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

