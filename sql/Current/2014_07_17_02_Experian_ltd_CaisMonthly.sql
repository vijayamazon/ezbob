SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('ExperianLtdCaisMonthly') IS NULL
BEGIN
	CREATE TABLE ExperianLtdCaisMonthly (
		ExperianLtdCaisMonthlyID BIGINT IDENTITY(1, 1) NOT NULL,
		ExperianLtdID BIGINT NOT NULL,
		NumberOfActiveAccounts DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_ExperianLtdCaisMonthlyID PRIMARY KEY (ExperianLtdCaisMonthlyID),
		CONSTRAINT FK_ExperianLtdCaisMonthly_ExperianLtdID FOREIGN KEY (ExperianLtdID) REFERENCES ExperianLtd(ExperianLtdID)
	)
END
GO

