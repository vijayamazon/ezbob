SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueLoadInputData') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueLoadInputData AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueLoadInputData
@CustomerID INT,
@Now DATETIME,
@MonthlyRepaymentOnly BIT
AS
BEGIN
	DECLARE @OriginID INT = (SELECT c.OriginID FROM Customer c WHERE c.Id = @CustomerID)

	IF ISNULL(@MonthlyRepaymentOnly, 0) = 0
	BEGIN
		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		DECLARE @CompanyID INT = dbo.udfGetCustomerCompanyID(@CustomerID, @Now)

		-------------------------------------------------------------------------

		SELECT
			RowType = 'CompanyRegistrationNumber',
			c.ExperianRefNum AS CompanyNumber
		FROM
			Company c
		WHERE
			c.Id = @CompanyID
			AND
			c.TypeOfBusiness IN ('Limited', 'LLP', 'PShip')
			AND
			c.ExperianRefNum IS NOT NULL
			AND
			c.ExperianRefNum NOT IN ('exception', 'NotFound')

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		SELECT TOP 1
			RowType = 'Address',
			a.Line1,
			a.Line2,
			Postcode = a.Rawpostcode
		FROM
			CustomerAddress a
		WHERE
			a.CustomerId = @CustomerID
			AND
			a.addressType = 1
			AND
			LTRIM(RTRIM(ISNULL(a.id, ''))) != ''

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

		SELECT
			RowType = 'DirectorData',
			c.FirstName,
			c.Surname,
			c.DateOfBirth,
			CompanyID = @CompanyID
		FROM
			Customer c
		WHERE
			c.Id = @CustomerID

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------

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

		-------------------------------------------------------------------------
		-------------------------------------------------------------------------
	
	END

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
		DefaultTerm = ISNULL(ls.DefaultRepaymentPeriod, 12),
		MaxInterestRate = ISNULL(ls.MaxInterest, 0.0225)
	FROM
		dbo.udfGetLoanSource(0, @OriginID) ls
		OUTER APPLY rq

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType = 'OpenLoan',
		LoanID = l.Id,
		l.LoanAmount,
		Term = ISNULL(ISNULL(l.CustomerSelectedTerm, ISNULL(r.ApprovedRepaymentPeriod, r.RepaymentPeriod)), 12),
		l.InterestRate
	FROM
		Loan l
		INNER JOIN CashRequests r ON l.RequestCashId = r.Id
	WHERE
		r.IdCustomer = @CustomerID
		AND
		l.[Date] < @Now
		AND
		(l.DateClosed IS NULL OR l.DateClosed > @Now)
END
GO
