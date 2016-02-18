SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadApplicationInfo') IS NULL
	EXECUTE('CREATE PROCEDURE LoadApplicationInfo AS SELECT 1')
GO

ALTER PROCEDURE LoadApplicationInfo
@CustomerID INT,
@CashRequestID BIGINT,
@Now DATETIME
AS
BEGIN
	------------------------------------------------------------------------------
	--
	-- Helper list: loan sources.
	--
	------------------------------------------------------------------------------

	DECLARE @DefaultLoanSourceID INT = (
		SELECT
			LoanSourceID
		FROM
			DefaultLoanSources dls
			INNER JOIN Customer c ON dls.OriginID = c.OriginID AND c.Id = @CustomerID
	)

	SELECT
		RowType = 'LoanSource',
		Id = ls.LoanSourceID,
		Name = ls.LoanSourceName,
		MaxInterest = ISNULL(ls.MaxInterest, -1),
		DefaultRepaymentPeriod = ISNULL(ls.DefaultRepaymentPeriod, -1),
		IsCustomerRepaymentPeriodSelectionAllowed = ISNULL(ls.IsCustomerRepaymentPeriodSelectionAllowed, -1),
		MaxEmployeeCount = ISNULL(ls.MaxEmployeeCount, -1),
		MaxAnnualTurnover = ISNULL(ls.MaxAnnualTurnover, -1),
		IsDefault = CONVERT(BIT, CASE WHEN ls.LoanSourceID = @DefaultLoanSourceID THEN 1 ELSE 0 END),
		AlertOnCustomerReasonType = ISNULL(ls.AlertOnCustomerReasonType, -1)
	FROM
		LoanSource ls

	------------------------------------------------------------------------------
	--
	-- Helper list: loan types.
	--
	------------------------------------------------------------------------------

	SELECT
		RowType = 'LoanType',
		Id = lt.Id,
		Name = lt.Name,
		value = lt.Id,
		text = lt.Name,
		RepaymentPeriod = lt.RepaymentPeriod
	FROM
		LoanType lt

	------------------------------------------------------------------------------
	--
	-- Helper list: discount plans.
	--
	------------------------------------------------------------------------------

	SELECT
		RowType = 'DiscountPlan',
		Id = dp.Id,
		Name = dp.Name,
		DiscountPlanPercents = CASE WHEN dp.ValuesStr LIKE '%[1-9]%' THEN '(' + dp.ValuesStr + ')' ELSE '' END
	FROM
		DiscountPlan dp

	------------------------------------------------------------------------------
	--
	-- Customer and cash request details - main model content.
	--
	------------------------------------------------------------------------------
	;WITH skip_aml AS (
		SELECT TOP 1
			CustomerID = a.CustomerId,
			DoNotShowAgain = a.DoNotShowAgain
		FROM
			ApprovalsWithoutAML a
		WHERE
			a.CustomerId = @CustomerID
		ORDER BY
			a.Timestamp DESC
	), cec AS (
		SELECT TOP 1
			CustomerID = cc.CustomerId,
			EmployeeCount = cc.EmployeeCount
		FROM
			CompanyEmployeeCount cc
			INNER JOIN Company co ON cc.CompanyId = co.Id
		WHERE
			cc.CustomerId = @CustomerID
		ORDER BY
			cc.Created DESC
	), request_reason AS (
		SELECT TOP 1
			CustomerID = crl.CustomerId,
			ReasonType = ISNULL(cr.ReasonType, -1),
			Reason = LTRIM(RTRIM(ISNULL(cr.Reason, ''))),
			OtherReason = LTRIM(RTRIM(ISNULL(crl.OtherReason, ''))),
			RequestedLoanAmount = crl.Amount,
			RequestedLoanTerm = crl.Term
		FROM
			CustomerRequestedLoan crl
			LEFT JOIN CustomerReason cr ON crl.ReasonId = cr.Id
		WHERE
			crl.CustomerId = @CustomerID
		ORDER BY
			crl.Created DESC
	), first_business_summary AS (
		SELECT
			s.SummaryID,
			Position = ROW_NUMBER() OVER (PARTITION BY s.BusinessID ORDER BY s.CreationDate DESC, s.SummaryID DESC)
		FROM
			MP_VatReturnSummary s
		WHERE
			s.CustomerID = @CustomerID
	), value_added_fcf AS (
		SELECT
			s.CustomerID,
			TotalValueAdded = SUM(s.TotalValueAdded),
			FreeCashFlow = SUM(s.FreeCashFlow)
		FROM
			MP_VatReturnSummary s
			INNER JOIN first_business_summary f ON s.SummaryID = f.SummaryID AND f.Position = 1
		GROUP BY
			s.CustomerID
	), origin_count AS (
		SELECT
			CustomerID = @CustomerID,
			OriginCount = COUNT(DISTINCT c.OriginID)
		FROM
			Customer c
			INNER JOIN Customer cc ON c.Name = cc.Name
		WHERE
			cc.Id = @CustomerID
	), num_of_loans AS (
		SELECT @CustomerId CustomerID, COUNT(*) NumOfLoans FROM Loan WHERE CustomerId=@CustomerID
	) SELECT
		RowType = 'Model',
		Id = c.Id,
		CustomerId = c.Id,
		CustomerName = c.Fullname,
		TypeOfBusiness = c.TypeOfBusiness,
		CustomerRefNum = c.RefNumber,
		LoanSourceID = ls.LoanSourceID,
		IsTest = c.IsTest,
		IsOffline = c.IsOffline,
		HasYodlee = CONVERT(BIT, CASE WHEN ISNULL((
			SELECT COUNT(DISTINCT m.Id)
			FROM MP_CustomerMarketPlace m
			INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
			WHERE m.CustomerId = c.Id
			AND t.InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
		), 0) > 0 THEN 1 ELSE 0 END),
		IsAvoid = c.AvoidAutomaticDescison,
		SystemDecision = c.Status,
		CreditResult = c.CreditResult,
		AvailableAmount = ISNULL(c.CreditSum, 0),
		OfferExpired = CONVERT(BIT, CASE WHEN c.ValidFor <= @Now THEN 1 ELSE 0 END),
		Editable = CONVERT(BIT, CASE WHEN s.IsEnabled = 1 AND c.CreditResult IN ('WaitingForDecision', 'Escalated', 'ApprovedPending', 'PendingInvestor') THEN 1 ELSE 0 END),
		IsCustomerInEnabledStatus = s.IsEnabled,
		IsModified = CONVERT(BIT, CASE WHEN r.Id IS NOT NULL AND ISNULL(r.LoanTemplate, '') != '' THEN 1 ELSE 0 END),
		DiscountPlanId = dp.DiscountPlanID,
		OfferValidForHours = CONVERT(INT, CONVERT(DECIMAL(18, 2), cv.Value)),
		AMLResult = c.AMLResult,
		SkipPopupForApprovalWithoutAML = ISNULL(skip_aml.DoNotShowAgain, 0),
		EmployeeCount = ISNULL(cec.EmployeeCount, 0),
		CustomerReasonType = ISNULL(rr.ReasonType, -1),
		CustomerReason = CASE WHEN rr.CustomerID IS NULL THEN '' ELSE
			CASE
				WHEN rr.Reason != '' AND rr.OtherReason != '' THEN rr.Reason + ': ' + rr.OtherReason
				WHEN rr.Reason  = '' AND rr.OtherReason != '' THEN rr.OtherReason
				WHEN rr.Reason != '' AND rr.OtherReason  = '' THEN rr.Reason
				ELSE ''
			END
		END,
		RequestedLoanAmount = rr.RequestedLoanAmount,
		RequestedLoanTerm = rr.RequestedLoanTerm,
		CashRequestId = ISNULL(r.Id, 0),
		CashRequestTimestamp = r.TimestampCounter,
		InterestRate = ISNULL(r.InterestRate, 0),
		SpreadSetupFee = ISNULL(r.SpreadSetupFee, 0),
		ManualSetupFeePercent = ISNULL(r.ManualSetupFeePercent, 0),
		BrokerSetupFeePercent = ISNULL(r.BrokerSetupFeePercent, 0),
		AllowSendingEmail = CASE WHEN r.Id IS NULL THEN CONVERT(BIT, 0) ELSE CONVERT(BIT, 1 - r.EmailSendingBanned) END,
		LoanTypeId = lt.LoanTypeID,
		OfferStart = ISNULL(r.OfferStart, c.ApplyForLoan),
		RawOfferStart = r.OfferStart,
		OfferValidUntil = ISNULL(r.OfferValidUntil, c.ValidFor),
		RepaymentPeriod = ISNULL(r.RepaymentPeriod, 0),
		SystemCalculatedAmount = CASE WHEN ISNULL(r.SystemCalculatedSum, 0) > 0.01 THEN ISNULL(r.SystemCalculatedSum, 0) ELSE 0 END,
		OfferedCreditLine = ISNULL(ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum), 0),
		BorrowedAmount = ISNULL((SELECT SUM(LoanAmount) FROM Loan WHERE RequestCashId = r.Id), 0),
		StartingFromDate = CASE WHEN r.OfferStart IS NULL THEN '' ELSE CONVERT(NVARCHAR, r.OfferStart, 103) END,
		OfferValidateUntil = CASE WHEN r.OfferValidUntil IS NULL THEN '' ELSE CONVERT(NVARCHAR, r.OfferValidUntil, 103) END,
		Reason = r.UnderwriterComment,
		IsLoanTypeSelectionAllowed = ISNULL(r.IsLoanTypeSelectionAllowed, 0),
		IsCustomerRepaymentPeriodSelectionAllowed = ISNULL(r.IsCustomerRepaymentPeriodSelectionAllowed, 0),
		AnnualTurnover = ISNULL(r.AnualTurnover, 0),
		ValueAdded = ISNULL(vf.TotalValueAdded, 0),
		FreeCashFlow = ISNULL(vf.FreeCashFlow, 0),
		BrokerID = c.BrokerID,
		BrokerCardID = (
			SELECT TOP 1 ci.Id
			FROM CardInfo ci
			WHERE ci.BrokerID = c.BrokerID
			AND ci.IsDefault = 1
		),
		IsMultiBranded = CONVERT(BIT, CASE ISNULL(oc.OriginCount, 0) WHEN 1 THEN 0 ELSE 1 END),
		OriginID = c.OriginID,
		ProductSubTypeID = r.ProductSubTypeID,
		NumOfLoans = nol.NumOfLoans,
		UwUpdatedFees = r.UwUpdatedFees
	FROM
		Customer c
		INNER JOIN CustomerStatuses s ON c.CollectionStatus = s.Id
		INNER JOIN ConfigurationVariables cv ON cv.Name = 'OfferValidForHours'
		INNER JOIN num_of_loans nol ON nol.CustomerID = c.Id
		LEFT JOIN CashRequests r ON c.Id = r.IdCustomer AND r.Id = @CashRequestID
		LEFT JOIN skip_aml ON r.IdCustomer = skip_aml.CustomerID
		LEFT JOIN cec ON c.Id = cec.CustomerID
		LEFT JOIN request_reason rr ON c.Id = rr.CustomerID
		LEFT JOIN value_added_fcf vf ON vf.CustomerID = r.IdCustomer
		LEFT JOIN origin_count oc ON oc.CustomerID = @CustomerID
		OUTER APPLY dbo.udfGetLoanSource(r.LoanSourceID, c.OriginID) ls
		OUTER APPLY dbo.udfGetDiscountPlan(r.DiscountPlanID) dp
		OUTER APPLY dbo.udfGetLoanType(r.LoanTypeId) lt
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------
	--
	-- Offer calculation.
	--
	------------------------------------------------------------------------------

	SELECT TOP 1
		RowType = 'OfferCalculation',
		Amount = oc.Amount,
		InterestRate = oc.InterestRate / 100.0,
		RepaymentPeriod = oc.Period,
		SetupFeePercent = oc.SetupFee / 100.0,
		SetupFeeAmount = oc.Amount / 100.0 * oc.SetupFee
	FROM
		OfferCalculations oc
	WHERE
		oc.CustomerId = @CustomerID
		AND
		oc.IsActive = 1
END
GO
