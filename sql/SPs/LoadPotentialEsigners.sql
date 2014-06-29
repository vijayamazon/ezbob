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
		'cutomer' AS Type,
		c.Id AS CustomerID,
		0 AS DirectorID,
		c.FirstName,
		c.Surname AS LastName,
		c.Name AS Email,
		s.StatusID,
		s.StatusName AS Status,
		CONVERT(BIT, 1) AS IsDirector,
		CONVERT(BIT, 1) AS IsShareholder
	FROM
		Customer c
		INNER JOIN EsignUserAgreementStatus s ON s.StatusID = 7
	WHERE
		@CustomerID IS NULL OR Id = @CustomerID
	UNION
	SELECT
		'director' AS Type,
		c.Id AS CustomerID,
		d.Id AS DirectorID,
		d.Name AS FirstName,
		d.Surname AS LastName,
		d.Email AS Email,
		s.StatusID,
		s.StatusName AS Status,
		CONVERT(BIT, 1) AS IsDirector,
		ISNULL(d.IsShareholder, 0) AS IsShareholder
	FROM
		Director d
		INNER JOIN Company co ON d.CompanyId = co.Id
		INNER JOIN Customer c ON co.Id = c.CompanyId AND (@CustomerID IS NULL OR c.Id = @CustomerID)
		INNER JOIN CustomerAddress a ON d.Id = a.DirectorId AND a.addressType IN (4, 6)
		INNER JOIN EsignUserAgreementStatus s ON s.StatusID = 7
	ORDER BY
		c.Id,
		1
END
GO
