IF OBJECT_ID('LoadPotentialEsigners') IS NULL
	EXECUTE('CREATE PROCEDURE LoadPotentialEsigners AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadPotentialEsigners
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'customer' AS Type,
		c.Id AS CustomerID,
		0 AS DirectorID,
		c.FirstName,
		c.Surname AS LastName,
		c.Name AS Email,
		0 AS UserId,
		c.MiddleInitial AS MiddleName,
		c.Gender AS Gender,
		s.StatusID,
		s.StatusName AS Status,
		CONVERT(BIT, 1) AS IsDirector,
		CONVERT(BIT, 1) AS IsShareholder,
		c.MobilePhone,
		a.Line1,
		a.Line2,
		a.Line3,
		a.Town,
		a.County,
		a.Postcode,
		c.DateOfBirth AS BirthDate
	FROM
		Customer c
		INNER JOIN EsignUserAgreementStatus s ON s.StatusID = 7
		INNER JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType = 1
	WHERE
		@CustomerID IS NULL OR c.Id = @CustomerID
	UNION
	SELECT
		'director' AS Type,
		c.Id AS CustomerID,
		d.Id AS DirectorID,
		d.Name AS FirstName,
		d.Surname AS LastName,
		d.Email AS Email,
		d.UserId AS UserId,  
		d.Middle AS MiddleName,
		d.Gender AS Gender,
		s.StatusID,
		s.StatusName AS Status,
		ISNULL(d.IsDirector, 0) AS IsDirector,
		ISNULL(d.IsShareholder, 0) AS IsShareholder,
		d.Phone AS MobilePhone,
		a.Line1,
		a.Line2,
		a.Line3,
		a.Town,
		a.County,
		a.Postcode,
		d.DateOfBirth AS BirthDate
	FROM
		Director d
		INNER JOIN Company co ON d.CompanyId = co.Id
		INNER JOIN Customer c ON co.Id = c.CompanyId AND (@CustomerID IS NULL OR c.Id = @CustomerID)
		INNER JOIN CustomerAddress a ON d.Id = a.DirectorId AND a.addressType IN (4, 6)
		INNER JOIN EsignUserAgreementStatus s ON s.StatusID = 7
	UNION
	SELECT
		'experian' AS Type,
		c.Id AS CustomerID,
		d.ExperianDirectorID AS DirectorID,
		d.FirstName AS FirstName,
		d.LastName AS LastName,
		d.Email AS Email,
		0 AS UserId,
		d.MiddleName AS MiddleName,
		d.Gender AS Gender,
		s.StatusID,
		s.StatusName AS Status,
		d.IsDirector AS IsDirector,
		d.IsShareholder AS IsShareholder,
		d.MobilePhone,
		d.Line1,
		d.Line2,
		d.Line3,
		d.Town,
		d.County,
		d.Postcode,
		d.BirthDate
	FROM
		ExperianDirectors d
		INNER JOIN Customer c ON c.Id = d.CustomerID AND (@CustomerID IS NULL OR c.Id = @CustomerID)
		INNER JOIN EsignUserAgreementStatus s ON s.StatusID = 7
	WHERE
		d.IsDeleted = 0
	ORDER BY
		c.Id,
		1
END
GO
