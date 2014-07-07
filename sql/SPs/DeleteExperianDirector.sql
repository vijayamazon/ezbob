IF OBJECT_ID('DeleteExperianDirector') IS NULL
	EXECUTE('CREATE PROCEDURE DeleteExperianDirector AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE DeleteExperianDirector
@DirectorID INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE ExperianDirectors SET
		IsDeleted = 1
	WHERE
		ExperianDirectorID = @DirectorID
END
GO
