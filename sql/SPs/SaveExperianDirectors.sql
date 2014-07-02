IF OBJECT_ID('SaveExperianDirectors') IS NULL
	EXECUTE('CREATE PROCEDURE SaveExperianDirectors AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveExperianDirectors
@DirList ExperianDirectorList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	SELECT
		CustomerID,
		RefNum,
		FirstName,
		MiddleName,
		LastName,
		BirthDate,
		Gender,
		IsDirector,
		IsShareholder,
		Line1,
		Line2,
		Line3,
		Town,
		County,
		Postcode,
		CONVERT(NVARCHAR(512), '') AS Email,
		CONVERT(NVARCHAR(512), '') AS MobilePhone
	INTO
		#t
	FROM
		@DirList

	------------------------------------------------------------------------------

	UPDATE #t SET
		Email = o.Email,
		MobilePhone = o.MobilePhone
	FROM
		#t n
		INNER JOIN ExperianDirectors o
			ON o.DirectorRefNum = n.RefNum
			AND o.CustomerID = n.CustomerID
			AND o.IsDeleted = 0

	------------------------------------------------------------------------------

	UPDATE ExperianDirectors SET
		IsDeleted = 1
	FROM
		ExperianDirectors o
		INNER JOIN #t n
			ON o.DirectorRefNum = n.RefNum
			AND o.CustomerID = n.CustomerID
			AND o.IsDeleted = 0

	------------------------------------------------------------------------------

	INSERT INTO ExperianDirectors (
		CustomerID, FirstName, MiddleName, LastName, BirthDate, Gender, Email, MobilePhone,
		IsDirector, IsShareholder, IsDeleted, DirectorRefNum,
		Line1, Line2, Line3, Town, County, Postcode
	) SELECT
		CustomerID, FirstName, MiddleName, LastName, BirthDate, Gender, Email, MobilePhone,
		IsDirector, IsShareholder, 0, RefNum,
		Line1, Line2, Line3, Town, County, Postcode
	FROM
		#t

	------------------------------------------------------------------------------

	DROP TABLE #t

	------------------------------------------------------------------------------

	COMMIT TRANSACTION

	------------------------------------------------------------------------------
END
GO
