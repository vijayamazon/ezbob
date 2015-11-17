SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadApplicationInfo') IS NULL
	EXECUTE('CREATE PROCEDURE LoadApplicationInfo AS SELECT 1')
GO

ALTER PROCEDURE LoadApplicationInfo
@CashRequestID BIGINT,
@Now DATETIME
AS
BEGIN
	-- TODO: LoanTypes
	-- TODO: LoanSources

	;WITH skip_aml AS (
		SELECT TOP 1
			CustomerID = a.CustomerId,
			DoNotShowAgain = a.DoNotShowAgain
		FROM
			ApprovalsWithoutAML a
			INNER JOIN CashRequests r
				ON a.CustomerId = r.IdCustomer
				AND r.Id = @CashRequestID
		ORDER BY
			a.Timestamp DESC
	), cec AS (
		SELECT TOP 1
			CustomerID = c.Id,
			EmployeeCount = cc.EmployeeCount
		FROM
			CompanyEmployeeCount cc
			INNER JOIN Company co ON cc.CompanyId = co.Id
			INNER JOIN Customer c ON cc.CustomerId = c.Id AND c.CompanyId = co.Id
			INNER JOIN CashRequests r ON cc.CustomerId = r.IdCustomer AND r.Id = @CashRequestID
		ORDER BY
			cc.Created DESC
	), request_reason AS (
		SELECT TOP 1
			CustomerID = crl.CustomerId,
			ReasonType = ISNULL(cr.ReasonType, -1),
			Reason = LTRIM(RTRIM(ISNULL(cr.Reason, ''))),
			OtherReason = LTRIM(RTRIM(ISNULL(crl.OtherReason, '')))
		FROM
			CustomerRequestedLoan crl
			INNER JOIN CashRequests r ON crl.CustomerId = r.IdCustomer AND r.Id = @CashRequestID
			INNER JOIN CustomerReason cr ON crl.ReasonId = cr.Id
		ORDER BY
			crl.Created DESC
	)
	SELECT
		CustomerID = r.IdCustomer,
		CustomerName = c.Fullname,
		TypeOfBusiness = c.TypeOfBusiness,
		CustomerRefNum = c.RefNumber,
		LoanSourceID = ls.LoanSourceID,
		LoanSource = ls.LoanSourceName,
		IsTest = c.IsTest,
		IsOffline = c.IsOffline,
		YodleeCount = (
			SELECT COUNT(DISTINCT m.Id)
			FROM MP_CustomerMarketPlace m
			INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
			WHERE m.CustomerId = r.IdCustomer
			AND t.InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
		),
		IsAvoid = c.AvoidAutomaticDescison,
		SystemDecision = c.Status,
		AvailableAmount = ISNULL(c.CreditSum, 0),
		OfferExpired = CONVERT(BIT, CASE WHEN c.ValidFor <= @Now THEN 1 ELSE 0 END),
		Editable = CONVERT(BIT, CASE WHEN s.IsEnabled = 1 AND c.CreditResult IN ('WaitingForDecision', 'Escalated', 'ApprovedPending') THEN 1 ELSE 0 END),
		IsModified = CONVERT(BIT, CASE WHEN ISNULL(r.LoanTemplate, '') != '' THEN 1 ELSE 0 END),
		DiscountPlan = dp.DiscountPlanName,
		DiscountPlanPercents = CASE WHEN dp.ValuesStr LIKE '%[1-9]%' THEN '(' + dp.ValuesStr + ')' ELSE '' END,
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
		CashRequestID = r.Id,
		CashRequestTimestamp = r.TimestampCounter,
		InterestRate = r.InterestRate,
		SpreadSetupFee = ISNULL(r.SpreadSetupFee, 0),
		ManualSetupFeePercent = r.ManualSetupFeePercent,
		BrokerSetupFeePercent = r.BrokerSetupFeePercent,
		AllowSendingEmail = CONVERT(BIT, 1 - r.EmailSendingBanned)
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id
		INNER JOIN CustomerStatuses s ON c.CollectionStatus = s.Id
		INNER JOIN ConfigurationVariables cv ON cv.Name = 'OfferValidForHours'
		LEFT JOIN skip_aml ON r.IdCustomer = skip_aml.CustomerID
		LEFT JOIN cec ON c.Id = cec.CustomerID
		LEFT JOIN request_reason rr ON c.Id = rr.CustomerID
		OUTER APPLY dbo.udfGetLoanSource(r.LoanSourceID) ls
		OUTER APPLY dbo.udfGetDiscountPlan(r.DiscountPlanID) dp
	WHERE
		r.Id = @CashRequestID
END
GO
