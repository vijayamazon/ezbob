IF OBJECT_ID('GetNonLimitedCompanyScoreHistory') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCompanyScoreHistory AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCompanyScoreHistory
	(@CustomerId INT,		
	 @RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		ExperianNonLimitedResultScoreHistory.RiskScore,
		ExperianNonLimitedResultScoreHistory.Date
	FROM 
		ExperianNonLimitedResults,
		ExperianNonLimitedResultScoreHistory
	WHERE 
		CustomerId = @CustomerId AND 
		RefNumber = @RefNumber AND 
		IsActive = 1 AND
		ExperianNonLimitedResults.Id = ExperianNonLimitedResultScoreHistory.ExperianNonLimitedResultId
END
GO
