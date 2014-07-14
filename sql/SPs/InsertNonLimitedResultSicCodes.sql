IF OBJECT_ID('InsertNonLimitedResultSicCodes') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultSicCodes AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultSicCodes
	(@ExperianNonLimitedResultId INT,
	 @Code NVARCHAR(5),
	 @Description NVARCHAR(200))
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultSicCodes
		(ExperianNonLimitedResultId, Code,	Description)
	VALUES
		(@ExperianNonLimitedResultId, @Code, @Description)
END
GO
