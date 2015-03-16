IF OBJECT_ID('RptCciData') IS NULL
	EXECUTE('CREATE PROCEDURE RptCciData AS SELECT 1')
GO

ALTER PROCEDURE RptCciData
AS
BEGIN
	SET NOCOUNT ON

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		l.Id AS LoanID,
		ISNULL(SUM(ISNULL(Amount, 0)), 0) AS Fees
	INTO
		#fees
	FROM
		LoanCharges ch
		INNER JOIN Loan l ON ch.LoanId = l.Id
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
			AND c.IsTest = 0
			AND
			-- c.CciMark = 1
			c.LimitedConsentToSearch = 1
		INNER JOIN ConfigurationVariables cv ON ch.ConfigurationVariableId = cv.Id
	WHERE
		cv.Name LIKE '%Charge'
	GROUP BY
		l.Id

	--------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		c.Id AS CustomerID,
		MAX(eb.Id) AS EbayID,
		CONVERT(INT, NULL) AS EbayDataID,
		COUNT(DISTINCT eb.Id) AS EbayCount,
		MAX(pp.Id) AS PaypalID,
		CONVERT(INT, NULL) AS PaypalDataID
	INTO
		#ebp
	FROM
		Customer c
		LEFT JOIN MP_CustomerMarketPlace eb ON c.Id = eb.CustomerId AND eb.MarketPlaceId = 1
		LEFT JOIN MP_CustomerMarketPlace pp ON c.Id = pp.CustomerId AND pp.MarketPlaceId = 3
	WHERE
		c.IsTest = 0
		AND
		-- c.CciMark = 1
		c.LimitedConsentToSearch = 1
	GROUP BY
		c.Id

	--------------------------------------------------------------------------
	--------------------------------------------------------------------------

	SELECT
		d.CustomerMarketPlaceId AS EbayID,
		MAX(d.Created) AS MaxDate
	INTO
		#eb
	FROM
		MP_EbayUserData d
		INNER JOIN #ebp ON d.CustomerMarketPlaceId = #ebp.EbayID
	GROUP BY
		d.CustomerMarketPlaceId

	--------------------------------------------------------------------------

	UPDATE #ebp SET
		EbayDataID = d.Id
	FROM
		MP_EbayUserData d,
		#eb
	WHERE
		d.CustomerMarketPlaceId = #eb.EbayID
		AND
		d.Created = #eb.MaxDate
		AND
		#ebp.EbayID = #eb.EbayID

	--------------------------------------------------------------------------
	--------------------------------------------------------------------------

	SELECT
		d.CustomerMarketPlaceId AS PaypalID,
		MAX(d.Updated) AS MaxDate
	INTO
		#pp
	FROM
		MP_PayPalPersonalInfo d
		INNER JOIN #ebp ON d.CustomerMarketPlaceId = #ebp.PaypalID
	GROUP BY
		d.CustomerMarketPlaceId

	--------------------------------------------------------------------------

	UPDATE #ebp SET
		PaypalDataID = d.Id
	FROM
		MP_PayPalPersonalInfo d,
		#pp
	WHERE
		d.CustomerMarketPlaceId = #pp.PaypalID
		AND
		d.Updated = #pp.MaxDate
		AND
		#ebp.PaypalID = #pp.PaypalID

	--------------------------------------------------------------------------
	--
	-- Main query
	--
	--------------------------------------------------------------------------

	SELECT
		c.TypeOfBusiness AS DebtorType,
		l.Id AS LoanID,
		CASE c.TypeOfBusiness
			WHEN 'SoleTrader' THEN 'Personal'
			ELSE 'Business'
		END AS LoanType,
		ISNULL(l.LoanAmount, 0) AS OriginalAmount,
		ISNULL(t.Amount, 0) - ISNULL(lc.Principal, 0) AS Principal,
		ISNULL(lc.Fees, 0) AS RepaidFees,
		ISNULL(f.Fees, 0) AS Fees,
		ISNULL(lc.Interest, 0) AS RepaidInterest,
		ISNULL(l.RefNum, '') AS LoanRef,
		ISNULL(l.Date, 'July 1 1976 0:0:0') AS DateOfAgreement,
		ISNULL((
			SELECT MAX(csh.TimeStamp)
			FROM CustomerStatusHistory csh
			INNER JOIN CustomerStatuses cs ON csh.NewStatus = cs.Id AND cs.Name = 'Default'
			WHERE csh.CustomerId = c.Id
		), 'July 1 1976') AS DateOfDefault,
		(CASE
			WHEN c.TypeOfBusiness IN ('Entrepreneur', 'PShip', 'PShip3P') THEN c.NonLimitedCompanyName
			WHEN c.TypeOfBusiness IN ('LLP', 'Limited') THEN c.LimitedCompanyName
			ELSE NULL
		END) AS CompanyName,
		c.Gender,
		c.FirstName,
		c.Surname AS LastName,
		c.DateOfBirth,
		c.MobilePhone,
		c.DaytimePhone,
		c.Name AS EmailApplicant,
		ISNULL(eba.Phone, '') AS EbayPhone,
		ISNULL(pp.Phone, '') AS PaypalPhone,
		ISNULL(eb.Email, '') AS EmailEbay,
		(CASE
			WHEN ebp.EbayCount > 1 THEN CONVERT(VARCHAR, ebp.EbayCount - 1) + ' more eBay account' + (CASE WHEN ebp.EbayCount > 2 THEN 's' ELSE '' END)
			ELSE ''
		END) AS HasOtherEbay,
		(
			ISNULL(a.Line1, '') +
			ISNULL(' ' + a.Line2, '') +
			ISNULL(' ' + a.Line3, '') +
			ISNULL(' ' + a.Town, '') +
			ISNULL(' ' + a.County, '') +
			ISNULL(' ' + a.Postcode, '') +
			ISNULL(' ' + a.Country, '')
		) AS CurrentAddress,
		(
			ISNULL(eba.Street, '') +
			ISNULL(' ' + eba.CityName, '') +
			ISNULL(' ' + eba.StateOrProvince, '') +
			ISNULL(' ' + eba.PostalCode, '') +
			ISNULL(' ' + eba.CountryName, '')
		) AS OfficeAddress,
		c.ResidentialStatus
	FROM
		Customer c
		INNER JOIN #ebp ebp ON c.Id = ebp.CustomerID
		LEFT JOIN Loan l ON c.Id = l.CustomerId
		LEFT JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Type = 'PacnetTransaction'
			AND t.Status = 'Done'
		LEFT JOIN CustomerAddress a
			ON c.Id = a.CustomerId
			AND a.addressType = 1
		LEFT JOIN MP_EbayUserData eb ON ebp.EbayDataID = eb.Id
		LEFT JOIN MP_EbayUserAddressData eba ON eb.RegistrationAddressId = eba.Id
		LEFT JOIN MP_PayPalPersonalInfo pp ON ebp.PaypalDataID = pp.Id
		LEFT JOIN (
			SELECT
				it.LoanId AS LoanID,
				SUM(it.LoanRepayment) AS Principal,
				SUM(it.Fees) AS Fees,
				SUM(it.Interest) AS Interest
			FROM
				LoanTransaction it
			WHERE
				it.Type = 'PaypointTransaction'
				AND
				it.Status = 'Done'
			GROUP BY
				it.LoanID
		) lc ON l.Id = lc.LoanID
		LEFT JOIN #fees f ON l.Id = f.LoanID
	WHERE
		c.IsTest = 0
		AND
		-- c.CciMark = 1
		c.LimitedConsentToSearch = 1
	ORDER BY
		c.Id,
		l.Id

	--------------------------------------------------------------------------

	DROP TABLE #fees
	DROP TABLE #pp
	DROP TABLE #eb
	DROP TABLE #ebp
END
GO
