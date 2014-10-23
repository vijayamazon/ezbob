IF OBJECT_ID('RptAlibabaDataSharing') IS NULL
	EXECUTE('CREATE PROCEDURE RptAlibabaDataSharing AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAlibabaDataSharing
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
	hmrc AS (
		SELECT
			Id
		FROM
			MP_MarketplaceType
		WHERE
			InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
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
			MAX(a.addressId) AS AddressID
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
		RowType  = 'MetaData',
		--
		--
		CreditAccount_CustomerID          = c.Id,
		CreditAccount_Email               = c.Name,
		CreditAccount_SecurityQuestion    = q.name,
		CreditAccount_SecurityAnswer      = u.SecurityAnswer1,
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
		ContactDetails_MobilePhone  = c.MobilePhone,
		ContactDetails_DaytimePhone = c.DaytimePhone,
		--
		--
		CompanyDetails_TypeOfBusiness            = b.TypeOfBusiness,
		CompanyDetails_IndustryType              = c.IndustryType,
		CompanyDetails_CompanyRegistrationNumber = b.CompanyNumber,
		CompanyDetails_CompanyName               = b.CompanyName,
		--
		--
		FinancialDetails_TotalAnnualRevenue = c.OverallTurnOver,
		FinancialDetails_EmployeeCount      = cec_data.EmployeeCount,
		FinancialDetails_TotalMonthlySalary = cec_data.TotalMonthlySalary,
		FinancialDetails_WhoFilesWithHmrc   = b.VatReporting,
		--
		--
		VatAccount_Linked   = CASE WHEN ISNULL(hmrc_linked.Counter,   0) > 0 THEN 'yes' ELSE 'no' END,
		VatAccount_Uploaded = CASE WHEN ISNULL(hmrc_uploaded.Counter, 0) > 0 THEN 'yes' ELSE 'no' END,
		--
		--
		ApprovalPhase_Verify_BuyerID       = c.ReferenceSource,
		ApprovalPhase_Verify_EzbobMemberID = c.RefNumber,
		ApprovalPhase_Verify_Email         = c.Name,
		ApprovalPhase_Verify_BusinessName  = b.CompanyName,
		--
		ApprovalPhase_Verify_BusinessAddress_Organisation = ba.Organisation,
		ApprovalPhase_Verify_BusinessAddress_Line1        = ba.Line1,
		ApprovalPhase_Verify_BusinessAddress_Line2        = ba.Line2,
		ApprovalPhase_Verify_BusinessAddress_Line3        = ba.Line3,
		ApprovalPhase_Verify_BusinessAddress_Town         = ba.Town,
		ApprovalPhase_Verify_BusinessAddress_County       = ba.County,
		ApprovalPhase_Verify_BusinessAddress_Postcode     = ba.Postcode,
		ApprovalPhase_Verify_BusinessAddress_Country      = ba.Country,
		ApprovalPhase_Verify_BusinessAddress_Pobox        = ba.Pobox,
		--
		ApprovalPhase_Verify_UnderCurrentOwnership = CASE b.TimeAtAddress
			WHEN 1 THEN '1 year'
			WHEN 2 THEN '2 years'
			WHEN 3 THEN '3 years or more'
			ELSE NULL
		END,
		ApprovalPhase_Verify_BusinessPhoneNumber   = b.BusinessPhone,
		ApprovalPhase_Verify_NumberOfEmployees     = cec_data.EmployeeCount,
		ApprovalPhase_Verify_MainIndustry          = cac.Sic1992Desc1,
		ApprovalPhase_Verify_Turnover              = c.OverallTurnOver,
		--
		--
		ApprovalPhase_Feedback_IsApproved     = CASE c.CreditResult
			WHEN 'Approved' THEN 'yes'
			WHEN 'Late' THEN 'yes'
			ELSE 'no'
		END,
		ApprovalPhase_Feedback_ApprovedAmount = CASE c.CreditResult
			WHEN 'Approved' THEN c.CreditSum
			WHEN 'Late' THEN c.CreditSum
			ELSE NULL
		END,
		ApprovalPhase_Feedback_ApprovalDate   = CASE c.CreditResult
			WHEN 'Approved' THEN c.DateApproved
			WHEN 'Late' THEN c.DateApproved
			ELSE NULL
		END,
		ApprovalPhase_Feedback_Remarks        = CASE
			WHEN c.CreditResult IN ('ApprovedPending', 'Escalated', 'WaitingForDecision') THEN 'Pending approval'
			WHEN c.CreditResult IS NULL THEN 'Did not finish application process'
			ELSE NULL
		END,
		ApprovalPhase_Feedback_RejectReason   = CASE c.CreditResult
			WHEN 'Rejected' THEN c.RejectedReason
			ELSE NULL
		END
		--
		--
	FROM
		Customer c
		INNER JOIN Security_User u
			ON c.Id = u.UserId
		INNER JOIN Security_Question q
			ON u.SecurityQuestion1Id = q.id
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
		LEFT JOIN CustomerAnalyticsCompany cac
			ON c.Id = cac.CustomerID
			AND cac.IsActive = 1
	WHERE
		c.Name LIKE 'alexbo%' -- TODO: put sourceref condition here
	
	------------------------------------------------------------------------------

	SELECT
		RowType    = 'Loan',
		CustomerID = c.Id,
		--
		--
		LoanAgreementPhase_OrderNo                = l.RefNum,
		LoanAgreementPhase_SingleFinancingAmount  = l.LoanAmount,
		LoanAgreementPhase_InterestRate           = l.InterestRate,
		LoanAgreementPhase_SingleFinancingTime    = l.CustomerSelectedTerm,
		--
		--
		LoanServicing_BuyerRepayDate = l.DateClosed,
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
		c.Name LIKE 'alexbo%' -- TODO: put sourceref condition here

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
			)
	WHERE
		c.Name LIKE 'alexbo%' -- TODO: put sourceref condition here
END
GO
