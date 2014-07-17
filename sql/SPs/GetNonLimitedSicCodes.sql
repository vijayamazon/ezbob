IF OBJECT_ID('GetNonLimitedSicCodes') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedSicCodes AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedSicCodes
	(@ExperianNonLimitedResultId INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Code,
		Description
	FROM 
		ExperianNonLimitedResultSicCodes 
	WHERE 
		ExperianNonLimitedResultId = @ExperianNonLimitedResultId
END
GO
