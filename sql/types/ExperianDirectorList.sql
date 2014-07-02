SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('ExperianDirectorList') IS NULL
BEGIN
	CREATE TYPE ExperianDirectorList AS TABLE (
		CustomerID INT NULL,
		RefNum NVARCHAR(512) NULL,
		FirstName NVARCHAR(512) NULL,
		MiddleName NVARCHAR(512) NULL,
		LastName NVARCHAR(512) NULL,
		BirthDate DATETIME NULL,
		Gender NCHAR(1) NULL,
		IsDirector BIT,
		IsShareholder BIT,
		Line1 NVARCHAR(512) NULL,
		Line2 NVARCHAR(512) NULL,
		Line3 NVARCHAR(512) NULL,
		Town NVARCHAR(512) NULL,
		County NVARCHAR(512) NULL,
		Postcode NVARCHAR(512) NULL
	)
END
GO
