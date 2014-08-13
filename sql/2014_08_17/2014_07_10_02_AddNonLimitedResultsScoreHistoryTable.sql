IF OBJECT_ID('ExperianNonLimitedResultsScoreHistory') IS NULL
BEGIN
	CREATE TABLE ExperianNonLimitedResultsScoreHistory (
		Id INT IDENTITY NOT NULL,
		NonLimitedResultId INT,
		RiskScore INT,
		Date DATETIME
	)
END
GO

