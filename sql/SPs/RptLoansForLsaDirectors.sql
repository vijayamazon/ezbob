IF OBJECT_ID('RptLoansForLsaDirectors') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaDirectors AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsaDirectors
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		LoanID = l.RefNum,
		LoanInternalID = l.Id,
		FirstName = d.Name,
		MiddleName = d.Middle,
		LastName = d.Surname,
		DateOfBirth = d.DateOfBirth,
		Gender = d.Gender,
		Email = d.Email,
		Phone = d.Phone,
		IsDirector = d.IsDirector,
		IsShareholder = d.IsShareholder,
		Address_Line1        = a.Line1,
		Address_Line2        = a.Line2,
		Address_Line3        = a.Line3,
		Address_Town         = a.Town,
		Address_County       = a.County,
		Address_Postcode     = a.Postcode,
		Address_Country      = a.Country
	FROM
		Loan l
		INNER JOIN PollenLoans lsa ON l.Id = lsa.LoanID
		INNER JOIN Customer c ON l.CustomerID = c.Id
		INNER JOIN Director d ON c.Id = d.CustomerId
		LEFT JOIN CustomerAddress a ON d.id = a.DirectorId
	WHERE
		d.IsDeleted = 0
END
GO
