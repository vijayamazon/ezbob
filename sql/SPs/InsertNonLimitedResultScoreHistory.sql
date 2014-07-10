IF OBJECT_ID('InsertNonLimitedResultScoreHistory') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultScoreHistory AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultScoreHistory
	(@NonLimitedResultId INT,
	 @RiskScore INT,
	 @Date DATETIME)
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultsScoreHistory
		(NonLimitedResultId, RiskScore,	Date)
	VALUES
		(@NonLimitedResultId, @RiskScore, @Date)
END
GO
