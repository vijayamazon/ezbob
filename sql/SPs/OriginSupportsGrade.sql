SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('OriginSupportsGrade') IS NULL
	EXECUTE('CREATE PROCEDURE OriginSupportsGrade AS SELECT 1')
GO

ALTER PROCEDURE OriginSupportsGrade
@CustomerID INT,
@GradeID INT,
@GradeOriginID INT OUTPUT
AS
BEGIN
	SET @GradeOriginID = 0

	SELECT
		@GradeOriginID = GradeOriginID
	FROM
		I_GradeOriginMap m
		INNER JOIN Customer c
			ON m.OriginID = c.OriginID
			AND c.Id = @CustomerID
	WHERE
		m.GradeID = @GradeID
			
END
GO
