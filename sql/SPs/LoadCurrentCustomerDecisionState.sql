IF OBJECT_ID('LoadCurrentCustomerDecisionState') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCurrentCustomerDecisionState AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[LoadCurrentCustomerDecisionState]
@UnderwriterID INT,
@CustomerID INT,
@CashRequestID BIGINT
AS
BEGIN
	;WITH u AS (
		SELECT
			UnderwriterID = u.UserId,
			UnderwriterName = u.UserName,
			ManagerRoleCount = (
				SELECT COUNT(*)
				FROM Security_UserRoleRelation ur
				INNER JOIN Security_Role r ON ur.RoleId = r.RoleId AND r.Name = 'manager'
				WHERE ur.UserId = u.UserId
			)
		FROM
			Security_User u
		WHERE
			UserId = @UnderwriterID
	), c AS (
		SELECT
			CustomerID = c.Id,
			CreditResult = c.CreditResult,
			NumOfPrevApprovals = c.NumApproves,
			NumOfPrevRejections = c.NumRejects,
			LastWizardStep = (SELECT TheLastOne FROM WizardStepTypes WHERE WizardStepTypeID = c.WizardStep),
			IsAlibaba = c.IsAlibaba,
			Email = c.Name,
			FilledByBroker = c.FilledByBroker,
			Origin = o.Name
		FROM
			Customer c LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
		WHERE
			c.Id = @CustomerID
	), r AS (
		SELECT
			CashRequestID = r.Id,
			CashRequestCustomerID = r.IdCustomer,
			DecisionStr = r.UnderwriterDecision,
			CashRequestTimestamp = r.TimestampCounter,
			OfferedCreditLine = ISNULL(ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum), 0),
			IsLoanTypeSelectionAllowed = r.IsLoanTypeSelectionAllowed,
			EmailSendingBanned = r.EmailSendingBanned,
			OfferValidUntil = r.OfferValidUntil,
			OfferStart = r.OfferStart,
			SpreadSetupFee = ISNULL(r.SpreadSetupFee, 0),
			ManualSetupFeePercent = ISNULL(r.ManualSetupFeePercent, 0),
			BrokerSetupFeePercent= ISNULL(r.BrokerSetupFeePercent, 0),
			r.InterestRate,
			r.DiscountPlanId,
			r.LoanSourceID,
			r.LoanTypeId,
			r.RepaymentPeriod,
			r.ApprovedRepaymentPeriod,
			r.IsCustomerRepaymentPeriodSelectionAllowed,
			r.CreationDate
		FROM
			CashRequests r
		WHERE
			r.Id = @CashRequestID
	)
	SELECT
		u.UnderwriterID,
		u.UnderwriterName,
		IsManager = CONVERT(BIT, CASE WHEN ISNULL(u.ManagerRoleCount, 0) > 0 THEN 1 ELSE 0 END),

		c.CustomerID,
		c.CreditResult,
		NumOfPrevApprovals = ISNULL(c.NumOfPrevApprovals, 0),
		NumOfPrevRejections = ISNULL(c.NumOfPrevRejections, 0),
		LastWizardStep = ISNULL(c.LastWizardStep, 0),
		IsAlibaba = ISNULL(c.IsAlibaba, 0),
		c.Email,
		c.Origin,
		FilledByBroker = ISNULL(c.FilledByBroker, 0),

		r.CashRequestID,
		r.CashRequestCustomerID,
		r.DecisionStr,
		r.CashRequestTimestamp,
		OfferedCreditLine = ISNULL(r.OfferedCreditLine, 0),
		IsLoanTypeSelectionAllowed = ISNULL(r.IsLoanTypeSelectionAllowed, 0),
		EmailSendingBanned = ISNULL(r.EmailSendingBanned, 0),
		r.OfferValidUntil,
		r.OfferStart,
		SpreadSetupFee = ISNULL(r.SpreadSetupFee, 0),
		ManualSetupFeePercent = ISNULL(r.ManualSetupFeePercent, 0),
		BrokerSetupFeePercent = ISNULL(r.BrokerSetupFeePercent, 0),
		r.InterestRate,
		r.DiscountPlanId as DiscountPlanID,
		r.LoanSourceID,
		r.LoanTypeId as LoanTypeID,
		r.RepaymentPeriod,
		r.ApprovedRepaymentPeriod,
		r.IsCustomerRepaymentPeriodSelectionAllowed,
		r.CreationDate
	FROM
		c
		FULL OUTER JOIN r ON 1 = 1
		FULL OUTER JOIN u ON 1 = 1
END

