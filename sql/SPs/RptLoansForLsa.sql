IF OBJECT_ID('RptLoansForLsa') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsa AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsa
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	;WITH
	crl_dates AS (
		SELECT
			CustomerId,
			MAX(Created) AS Created
		FROM
			CustomerRequestedLoan
		GROUP BY
			CustomerId
	),
	crl AS (
		SELECT
			c.CustomerId,
			MAX(c.Id) AS CrlID
		FROM
			crl_dates d
			INNER JOIN CustomerRequestedLoan c
				ON d.CustomerId = c.CustomerId
				AND d.Created = c.Created
		GROUP BY
			c.CustomerId
	),
	cec_dates AS (
		SELECT
			CustomerId,
			MAX(Created) AS Created
		FROM
			CompanyEmployeeCount
		GROUP BY
			CustomerID
	),
	cec AS (
		SELECT
			c.CustomerId AS CustomerID,
			MAX(c.Id) AS CecID
		FROM
			cec_dates d
			INNER JOIN CompanyEmployeeCount c
				ON d.CustomerId = c.CustomerId
				AND d.Created = c.Created
		GROUP BY
			c.CustomerId
	),
	comp_addr_types AS (
		SELECT DISTINCT
			c.Id AS CustomerID,
			ISNULL(a.addressType, 1) AS AddressType
		FROM
			Customer c
			LEFT JOIN Company b
				ON c.CompanyId = b.Id
			LEFT JOIN CustomerAddress a
				ON c.Id = a.CustomerId
				AND a.addressType = CASE
					WHEN b.TypeOfBusiness IN ('Limited', 'LLP') THEN 3
					WHEN b.TypeOfBusiness IN ('SoleTrader', 'PShip', 'PShip3P') THEN 5
					ELSE 1
				END
	),
	comp_addr_ids AS (
		SELECT
			t.CustomerID,
			t.AddressType,
			MAX(ISNULL(a.addressId, 0)) AS AddressID
		FROM
			comp_addr_types t
			LEFT JOIN CustomerAddress a
				ON t.CustomerID = a.CustomerId
				AND t.AddressType = a.addressType
		GROUP BY
			t.CustomerID,
			t.AddressType
	)
	SELECT
		LoanID         = l.RefNum,
		LoanInternalID = l.Id,
		--
		--
		CustomerRefnum      = c.RefNumber,
		Email               = c.Name,
		PromoCode           = c.PromoCode,
		RequestedLoanAmount = crl_data.Amount,
		--
		--
		FirstName     = c.FirstName,
		MiddleName    = c.MiddleInitial,
		LastName      = c.Surname,
		Gender        = c.Gender,
		DateOfBirth   = c.DateOfBirth,
		MaritalStatus = c.MaritalStatus,
		--
		--
		PostCode       = ha.Postcode,
		TimeAttAddress = CASE c.TimeAtAddress
			WHEN 1 THEN '1 year'
			WHEN 2 THEN '2 years'
			WHEN 3 THEN '3 years or more'
			ELSE 'not specified'
		END,
		--
		--
		MobilePhone  = c.MobilePhone,
		DaytimePhone = c.DaytimePhone,
		--
		--
		TypeOfBusiness            = b.TypeOfBusiness,
		IndustryType              = c.IndustryType,
		CompanyRegistrationNumber = b.CompanyNumber,
		CompanyName               = b.CompanyName,
		MonthsOfOperation          = CASE
			WHEN b.TypeOfBusiness IN ('Limited', 'LLP') THEN dbo.udfAgeInYM((SELECT IncorporationDate FROM dbo.udfGetCustomerCompanyAnalytics(c.Id, NULL, 0, 0, 0)))
			WHEN b.TypeOfBusiness NOT IN ('Limited', 'LLP') THEN dbo.udfAgeInYM(nl.IncorporationDate)
			ELSE ''
		END,
		--
		--
		EmployeeCount          = cec_data.EmployeeCount,
		TotalMonthlySalary     = cec_data.TotalMonthlySalary,
		WhoFilesWithHmrc       = b.VatReporting,
		--
		--
		BusinessAddress_Organisation = ba.Organisation,
		BusinessAddress_Line1        = ba.Line1,
		BusinessAddress_Line2        = ba.Line2,
		BusinessAddress_Line3        = ba.Line3,
		BusinessAddress_Town         = ba.Town,
		BusinessAddress_County       = ba.County,
		BusinessAddress_Postcode     = ba.Postcode,
		BusinessAddress_Country      = ba.Country,
		--
		UnderCurrentOwnership = CASE b.TimeAtAddress
			WHEN 1 THEN '1 year'
			WHEN 2 THEN '2 years'
			WHEN 3 THEN '3 years or more'
			ELSE NULL
		END,
		BusinessPhoneNumber    = b.BusinessPhone,
		NumberOfEmployees      = cec_data.EmployeeCount,
		MainIndustry           = (SELECT Sic1992Desc1 FROM dbo.udfGetCustomerCompanyAnalytics(c.Id, NULL, 0, 0, 0))
		--
		--
	FROM
		Loan l
		INNER JOIN PollenLoans lsa ON l.Id = lsa.LoanID
		INNER JOIN Customer c ON l.CustomerID = c.Id
		LEFT JOIN crl
			ON c.Id = crl.CustomerId
		LEFT JOIN CustomerRequestedLoan crl_data
			ON crl.CrlId = crl_data.Id
		LEFT JOIN CustomerAddress ha
			ON c.Id = ha.CustomerId AND ha.addressType = 1
		LEFT JOIN Company b
			ON c.CompanyId = b.Id
		LEFT JOIN cec
			ON c.Id = cec.CustomerID
		LEFT JOIN CompanyEmployeeCount cec_data
			ON cec.CecID = cec_data.Id
		LEFT JOIN comp_addr_ids cai
			ON c.Id = cai.CustomerID
		LEFT JOIN CustomerAddress ba
			ON cai.AddressID = ba.addressId
		LEFT JOIN CustomerManualUwData muw
			ON c.Id = muw.CustomerID
			AND muw.IsActive = 1
		LEFT JOIN ExperianNonLimitedResults nl
			ON b.ExperianRefNum = nl.RefNumber
			AND nl.IsActive = 1			

	------------------------------------------------------------------------------
END
GO
