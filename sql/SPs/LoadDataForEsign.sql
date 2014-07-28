IF OBJECT_ID('LoadDataForEsign') IS NULL
	EXECUTE('CREATE PROCEDURE LoadDataForEsign AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadDataForEsign
@CustomerID INT,
@TemplateID INT,
@Directors IntList READONLY,
@ExperianDirectors IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SELECT
		'Customer' AS RowType,
		c.Id AS ID,
		c.FirstName AS FirstName,
		c.Surname AS LastName,
		c.Name AS Email,
		a.Line1,
		a.Line2,
		a.Line3,
		a.Town,
		a.County,
		a.Postcode,
		a.Country
	FROM
		Customer c
		INNER JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType = 1
	WHERE
		c.Id = @CustomerID
	ORDER BY
		a.addressId

	------------------------------------------------------------------------------

	SELECT
		'Director' AS RowType,
		d.Id AS ID,
		d.Name AS FirstName,
		d.Surname AS LastName,
		d.Email AS Email,
		a.Line1,
		a.Line2,
		a.Line3,
		a.Town,
		a.County,
		a.Postcode,
		a.Country
	FROM
		Director d
		INNER JOIN CustomerAddress a ON d.Id = a.DirectorId AND a.addressType IN (4, 6)
		INNER JOIN @Directors dl ON d.Id = dl.Value
	ORDER BY
		a.addressId

	------------------------------------------------------------------------------

	SELECT
		'ExperianDirector' AS RowType,
		d.ExperianDirectorID AS ID,
		d.FirstName AS FirstName,
		d.LastName AS LastName,
		d.Email AS Email,
		d.Line1,
		d.Line2,
		d.Line3,
		d.Town,
		d.County,
		d.Postcode,
		'' AS Country
	FROM
		ExperianDirectors d
		INNER JOIN @ExperianDirectors dl ON d.ExperianDirectorID = dl.Value

	------------------------------------------------------------------------------

	SELECT
		'Company' AS RowType,
		co.Id AS ID,
		co.CompanyName AS Name,
		a.Line1,
		a.Line2,
		a.Line3,
		a.Town,
		a.County,
		a.Postcode,
		a.Country
	FROM
		Company co
		INNER JOIN Customer c ON co.Id = c.CompanyId AND c.Id = @CustomerID
		INNER JOIN CustomerAddress a ON co.Id = a.CompanyId AND a.addressType IN (3, 5)
	ORDER BY
		a.addressId

	------------------------------------------------------------------------------

	SELECT
		'Template' AS RowType,
		t.EsignTemplateID AS ID,
		tt.EsignTemplateTypeID AS TypeID,
		tt.DocumentName,
		tt.FileNameBase,
		t.FileExtension,
		t.Template AS FileContent
	FROM
		EsignTemplates t
		INNER JOIN EsignTemplateTypes tt ON t.EsignTemplateTypeID = tt.EsignTemplateTypeID
	WHERE
		t.EsignTemplateID = @TemplateID

	------------------------------------------------------------------------------
END
GO
