IF OBJECT_ID('InsertNonLimitedResultCcjRegisteredAgainst') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultCcjRegisteredAgainst AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultCcjRegisteredAgainst
	(@ExperianNonLimitedResultCcjDetailsId INT,
	 @Name INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultCcjRegisteredAgainst
		(ExperianNonLimitedResultCcjDetailsId, Name)
	VALUES
		(@ExperianNonLimitedResultCcjDetailsId, @Name)
END
GO
