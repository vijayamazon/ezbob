IF OBJECT_ID('GetNonLimitedCcjDetailRegisteredAgainst') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCcjDetailRegisteredAgainst AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCcjDetailRegisteredAgainst
	(@ExperianNonLimitedResultCcjDetailsId INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Name
	FROM 
		ExperianNonLimitedResultCcjRegisteredAgainst 
	WHERE 
		ExperianNonLimitedResultCcjDetailsId = @ExperianNonLimitedResultCcjDetailsId
END
GO
