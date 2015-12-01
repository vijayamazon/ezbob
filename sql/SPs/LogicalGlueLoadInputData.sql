SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueLoadInputData') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadInputData AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueLoadInputData
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @CompanyID INT = NULL

	------------------------------------------------------------------------------

	SELECT TOP 1
		@CompanyID = h.CompanyID
	FROM
		CustomerCompanyHistory h
	WHERE
		h.InsertDate < @Now
		AND
		h.CustomerId = @CustomerID
	ORDER BY
		h.InsertDate DESC,
		h.Id DESC

	------------------------------------------------------------------------------

	IF @CompanyID IS NULL
		SELECT @CompanyID = c.CompanyID FROM Customer c WHERE c.Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT
		RowType = 'CompanyRegistrationNumber',
		c.CompanyNumber
	FROM
		Company c
	WHERE
		c.Id = @CompanyID
		AND
		c.TypeOfBusiness IN ('Limited', 'LLP')
		AND
		c.ExperianRefNum IS NOT NULL
		AND
		c.ExperianRefNum NOT IN ('exception', 'NotFound')

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	;WITH rq AS (
		SELECT TOP 1
			Amount = crl.Amount,
			Term = crl.Term
		FROM
			CustomerRequestedLoan crl
		WHERE
			crl.CustomerID = @CustomerID
			AND
			crl.Created < @Now
		ORDER BY
			crl.Created DESC,
			crl.Id DESC
	)
	SELECT TOP 1
		RowType = 'RequestedLoan',
		Amount = ISNULL(rq.Amount, dbo.udfGetAvergateLoanAmount()),
		Term = ISNULL(ISNULL(rq.Term, ls.DefaultRepaymentPeriod), 12),
		DefaultAmount = dbo.udfGetAvergateLoanAmount(),
		DefaultTerm = ISNULL(ls.DefaultRepaymentPeriod, 12)
	FROM
		dbo.udfGetLoanSource(0) ls
		OUTER APPLY rq

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType = 'DirectorData',
		c.FirstName,
		c.Surname,
		c.DateOfBirth
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType = 'EquifaxData',
		l.ResponseData
	FROM
		LogicalGlueResponses r
		INNER JOIN MP_ServiceLog l ON r.ServiceLogID = l.Id
	WHERE
		r.HasEquifaxData = 1
		AND
		l.CustomerId = @CustomerID
		AND
		l.CompanyID = @CompanyID
	ORDER BY
		r.ReceivedTime DESC,
		r.ResponseID DESC
END
GO
