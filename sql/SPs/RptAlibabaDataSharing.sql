IF OBJECT_ID('RptAlibabaDataSharing') IS NULL
	EXECUTE('CREATE PROCEDURE RptAlibabaDataSharing AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAlibabaDataSharing
@IncludeTest BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

	SET @IncludeTest = ISNULL(@IncludeTest, 0)

	DECLARE @Hmrc UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	------------------------------------------------------------------------------

	SELECT DISTINCT
		RowType = 'IsLate',
		lst.LoanID
	FROM
		Loan l
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
		INNER JOIN LoanScheduleTransaction lst
			ON l.Id = lst.LoanId
			AND (
				lst.StatusBefore IN ('Late', 'Paid')
				OR
				lst.StatusAfter IN ('Late', 'Paid')
				OR
				l.Status = 'Late'
			)
	WHERE
		c.AlibabaId IS NOT NULL
		AND (
			@IncludeTest = 1
			OR
			c.IsTest = 0
		)

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
	hmrc AS (
		SELECT
			Id
		FROM
			MP_MarketplaceType
		WHERE
			InternalId = @Hmrc
	),
	hmrc_linked AS (
		SELECT
			c.Id AS CustomerID,
			COUNT(*) AS Counter
		FROM
			Customer c
			INNER JOIN MP_CustomerMarketPlace m
				ON c.Id = m.CustomerId
				AND c.Name != m.DisplayName
			INNER JOIN hmrc
				ON m.MarketPlaceId = hmrc.Id
		GROUP BY
			c.Id
	),
	hmrc_uploaded AS (
		SELECT
			c.Id AS CustomerID,
			COUNT(*) AS Counter
		FROM
			Customer c
			INNER JOIN MP_CustomerMarketPlace m
				ON c.Id = m.CustomerId
				AND c.Name = m.DisplayName
			INNER JOIN hmrc
				ON m.MarketPlaceId = hmrc.Id
		GROUP BY
			c.Id
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
		RowType    = 'MetaData',
		CustomerID = c.Id,
		AlibabaID  = c.AlibabaId,
		CustomerRefnum = c.RefNumber,
		--
		--
		CreditAccount_Email               = c.Name,
		CreditAccount_PromoCode           = c.PromoCode,
		CreditAccount_RequestedLoanAmount = crl_data.Amount,
		CreditAccount_MobilePhone         = c.MobilePhone,
		--
		--
		PersonalInfo_FirstName     = c.FirstName,
		PersonalInfo_MiddleName    = c.MiddleInitial,
		PersonalInfo_LastName      = c.Surname,
		PersonalInfo_Gender        = c.Gender,
		PersonalInfo_DateOfBirth   = c.DateOfBirth,
		PersonalInfo_MaritalStatus = c.MaritalStatus,
		--
		--
		HomeAddress_PostCode       = ha.Postcode,
		HomeAddress_TimeAttAddress = CASE c.TimeAtAddress
			WHEN 1 THEN '1 year'
			WHEN 2 THEN '2 years'
			WHEN 3 THEN '3 years or more'
			ELSE 'not specified'
		END,
		--
		--
		PersonalContactDetails_MobilePhone  = c.MobilePhone,
		PersonalContactDetails_DaytimePhone = c.DaytimePhone,
		--
		--
		CompanyDetails_TypeOfBusiness            = b.TypeOfBusiness,
		CompanyDetails_IndustryType              = c.IndustryType,
		CompanyDetails_CompanyRegistrationNumber = b.CompanyNumber,
		CompanyDetails_CompanyName               = b.CompanyName,
		CompanyDetails_MonthsOfOperation          = CASE
			WHEN b.TypeOfBusiness IN ('Limited', 'LLP') THEN dbo.udfAgeInYM((SELECT IncorporationDate FROM dbo.udfGetCustomerCompanyAnalytics(c.Id, NULL, 0, 0, 0)))
			WHEN b.TypeOfBusiness NOT IN ('Limited', 'LLP') THEN dbo.udfAgeInYM(nl.IncorporationDate)
			ELSE ''
		END,
		--
		--
		FinancialDetails_TotalAnnualizedRevenue = muw.AnnualizedRevenue,
		FinancialDetails_EmployeeCount          = cec_data.EmployeeCount,
		FinancialDetails_TotalMonthlySalary     = cec_data.TotalMonthlySalary,
		FinancialDetails_WhoFilesWithHmrc       = b.VatReporting,
		--
		--
		DataPoint_VatLinked   = CASE WHEN ISNULL(hmrc_linked.Counter,   0) > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
		DataPoint_VatUploaded = CASE WHEN ISNULL(hmrc_uploaded.Counter, 0) > 0 THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END,
		--
		--
		ApprovalPhaseVerify_Email         = c.Name,
		ApprovalPhaseVerify_BusinessName  = b.CompanyName,
		--
		ApprovalPhaseVerify_BusinessAddress_Organisation = ba.Organisation,
		ApprovalPhaseVerify_BusinessAddress_Line1        = ba.Line1,
		ApprovalPhaseVerify_BusinessAddress_Line2        = ba.Line2,
		ApprovalPhaseVerify_BusinessAddress_Line3        = ba.Line3,
		ApprovalPhaseVerify_BusinessAddress_Town         = ba.Town,
		ApprovalPhaseVerify_BusinessAddress_County       = ba.County,
		ApprovalPhaseVerify_BusinessAddress_Postcode     = ba.Postcode,
		ApprovalPhaseVerify_BusinessAddress_Country      = ba.Country,
		ApprovalPhaseVerify_BusinessAddress_Pobox        = ba.Pobox,
		--
		ApprovalPhaseVerify_UnderCurrentOwnership = CASE b.TimeAtAddress
			WHEN 1 THEN '1 year'
			WHEN 2 THEN '2 years'
			WHEN 3 THEN '3 years or more'
			ELSE NULL
		END,
		ApprovalPhaseVerify_BusinessPhoneNumber    = b.BusinessPhone,
		ApprovalPhaseVerify_NumberOfEmployees      = cec_data.EmployeeCount,
		ApprovalPhaseVerify_MainIndustry           = (SELECT Sic1992Desc1 FROM dbo.udfGetCustomerCompanyAnalytics(c.Id, NULL, 0, 0, 0)),
		ApprovalPhaseVerify_TotalAnnualizedRevenue = muw.AnnualizedRevenue,
		--
		--
		ApprovalPhaseFeedback_IsApproved     = CASE 
			WHEN c.CreditResult IN ('Approved', 'Late') THEN 'yes'
			WHEN c.CreditResult IN ('ApprovedPending', 'Escalated', 'WaitingForDecision') THEN 'pending'
			WHEN c.CreditResult IS NULL THEN 'n/a'
			ELSE 'no'
		END,
		ApprovalPhaseFeedback_ApprovedAmount = CASE c.CreditResult
			WHEN 'Approved' THEN c.CreditSum
			WHEN 'Late' THEN c.CreditSum
			ELSE NULL
		END,
		ApprovalPhaseFeedback_ApprovalDate   = CASE c.CreditResult
			WHEN 'Approved' THEN c.DateApproved
			WHEN 'Late' THEN c.DateApproved
			ELSE NULL
		END,
		ApprovalPhaseFeedback_Remarks        = CASE
			WHEN c.CreditResult IN ('Escalated', 'WaitingForDecision') THEN 'Under review'
			WHEN c.CreditResult = 'ApprovedPending' THEN 'Waiting for VAT or bank statements'
			WHEN c.CreditResult IS NULL THEN 'Did not finish application process'
			ELSE NULL
		END,
		ApprovalPhaseFeedback_RejectReason   = CASE c.CreditResult
			WHEN 'Rejected' THEN c.RejectedReason
			ELSE NULL
		END
		--
		--
	FROM
		Customer c
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
		LEFT JOIN hmrc_linked
			ON c.Id = hmrc_linked.CustomerID
		LEFT JOIN hmrc_uploaded
			ON c.Id = hmrc_uploaded.CustomerID
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
	WHERE
		c.AlibabaId IS NOT NULL
		AND
		c.DateOfBirth IS NOT NULL
		AND (
			@IncludeTest = 1
			OR
			c.IsTest = 0
		)

	------------------------------------------------------------------------------

	SELECT DISTINCT
		RowType     = 'Marketplace',
		CustomerID  = c.Id,
		Marketplace = m.Name
	FROM
		Customer c
		INNER JOIN MP_CustomerMarketPlace cmp ON c.Id = cmp.CustomerId
		INNER JOIN MP_MarketplaceType m
			ON cmp.MarketPlaceId = m.Id
			AND m.InternalId != @Hmrc
	WHERE
		c.AlibabaId IS NOT NULL
		AND
		c.DateOfBirth IS NOT NULL
		AND (
			@IncludeTest = 1
			OR
			c.IsTest = 0
		)

	------------------------------------------------------------------------------

	SELECT
		RowType    = 'Loan',
		CustomerID = c.Id,
		--
		--
		LoanAgreementPhase_OrderNo                = l.RefNum,
		LoanAgreementPhase_SingleFinancingAmount  = l.LoanAmount,
		LoanAgreementPhase_InterestRate           = l.InterestRate,
		LoanAgreementPhase_SingleFinancingTime    = l.CustomerSelectedTerm
		--
		--
	FROM
		Loan l
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
	WHERE
		c.AlibabaId IS NOT NULL
		AND
		c.DateOfBirth IS NOT NULL
		AND (
			@IncludeTest = 1
			OR
			c.IsTest = 0
		)

	------------------------------------------------------------------------------

	SELECT
		RowType    = 'Repayment',
		CustomerID = c.Id,
		--
		--
		LoanID = l.Id,
		LoanRefNum = l.RefNum,
		l.LoanAmount,
		l.DateClosed,
		t.Amount,
		t.Interest,
		l.RequestCashId,
		r.ManagerApprovedSum
		--
		--
	FROM
		Loan l
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
		INNER JOIN CashRequests r ON
			l.RequestCashId = r.Id
		LEFT JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Type = 'PaypointTransaction'
			ANd t.Status = 'Done'
	WHERE
		c.AlibabaId IS NOT NULL
		AND
		c.DateOfBirth IS NOT NULL
		AND (
			@IncludeTest = 1
			OR
			c.IsTest = 0
		)

	------------------------------------------------------------------------------

	SELECT DISTINCT
		RowType = 'AlibabaID',
		Source  = a.Source
	FROM
		SiteAnalytics a
	WHERE
		a.Source LIKE '/alibaba%'
		AND
		a.Source LIKE '%alibaba_id=%'

	------------------------------------------------------------------------------
END
GO
