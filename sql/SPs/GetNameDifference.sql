IF OBJECT_ID('GetNameDifference') IS NULL
	EXECUTE('CREATE PROCEDURE GetNameDifference AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetNameDifference
@FirstName NVARCHAR(500),
@LastName NVARCHAR(500),
@FirstNames StringList READONLY,
@LastNames StringList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Alice = @FirstName,
		Boob = v.Value,
		Mark = DIFFERENCE(@FirstName, v.Value)
	FROM
		@FirstNames v
	UNION
	SELECT
		Alice = @LastName,
		Boob = v.Value,
		Mark = DIFFERENCE(@LastName, v.Value)
	FROM
		@LastNames v
END
GO
