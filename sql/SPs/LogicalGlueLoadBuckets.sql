SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueLoadBuckets') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadBuckets AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueLoadBuckets
AS
BEGIN
	SELECT
		Value = GradeID,
		Name
	FROM
		I_Grade
END
GO
