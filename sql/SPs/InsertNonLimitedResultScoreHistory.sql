IF OBJECT_ID('InsertNonLimitedResultScoreHistory') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultScoreHistory AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultScoreHistory
	(@ExperianNonLimitedResultId INT,
	 @RiskScore INT,
	 @Date DATETIME)
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultScoreHistory
		(ExperianNonLimitedResultId, RiskScore,	Date)
	VALUES
		(@ExperianNonLimitedResultId, @RiskScore, @Date)
END
GO
