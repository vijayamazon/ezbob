SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RestoreResetCashRequest') IS NULL
	EXECUTE('CREATE PROCEDURE RestoreResetCashRequest AS SELECT 1')
GO

ALTER PROCEDURE RestoreResetCashRequest
@CashRequestID BIGINT
AS
BEGIN
	DECLARE @CashRequestHistoryID BIGINT

	------------------------------------------------------------------------------

	SELECT
		@CashRequestHistoryID = MIN(CashRequestHistoryID)
	FROM
		CashRequestsHistory h
	WHERE
		h.Id = @CashRequestID
		AND
		h.UnderwriterDecision IN ('Approved', 'Rejected')

	------------------------------------------------------------------------------

	IF @CashRequestHistoryID IS NOT NULL
	BEGIN
		UPDATE CashRequests SET
			IdCustomer                                 = h.IdCustomer,
			IdUnderwriter                              = h.IdUnderwriter,
			CreationDate                               = h.CreationDate,
			SystemDecision                             = h.SystemDecision,
			UnderwriterDecision                        = h.UnderwriterDecision,
			SystemDecisionDate                         = h.SystemDecisionDate,
			UnderwriterDecisionDate                    = h.UnderwriterDecisionDate,
			EscalatedDate                              = h.EscalatedDate,
			SystemCalculatedSum                        = h.SystemCalculatedSum,
			ManagerApprovedSum                         = h.ManagerApprovedSum,
			MedalType                                  = h.MedalType,
			EscalationReason                           = h.EscalationReason,
			APR                                        = h.APR,
			RepaymentPeriod                            = h.RepaymentPeriod,
			ScorePoints                                = h.ScorePoints,
			ExpirianRating                             = h.ExpirianRating,
			AnualTurnover                              = h.AnualTurnover,
			InterestRate                               = h.InterestRate,
			UseSetupFee                                = h.UseSetupFee,
			EmailSendingBanned                         = h.EmailSendingBanned,
			LoanTypeId                                 = h.LoanTypeId,
			UnderwriterComment                         = h.UnderwriterComment,
			HasLoans                                   = h.HasLoans,
			LoanTemplate                               = h.LoanTemplate,
			IsLoanTypeSelectionAllowed                 = h.IsLoanTypeSelectionAllowed,
			DiscountPlanId                             = h.DiscountPlanId,
			LoanSourceID                               = h.LoanSourceID,
			OfferStart                                 = h.OfferStart,
			OfferValidUntil                            = h.OfferValidUntil,
			IsCustomerRepaymentPeriodSelectionAllowed  = h.IsCustomerRepaymentPeriodSelectionAllowed,
			UseBrokerSetupFee                          = h.UseBrokerSetupFee,
			ManualSetupFeeAmount                       = h.ManualSetupFeeAmount,
			ManualSetupFeePercent                      = h.ManualSetupFeePercent,
			QuickOfferID                               = h.QuickOfferID,
			Originator                                 = h.Originator,
			ApprovedRepaymentPeriod                    = h.ApprovedRepaymentPeriod,
			AutoDecisionID                             = h.AutoDecisionID,
			BrokerSetupFeePercent                      = h.BrokerSetupFeePercent,
			SpreadSetupFee                             = h.SpreadSetupFee,
			HasApprovalChance                          = h.HasApprovalChance
		FROM
			CashRequests r
			INNER JOIN CashRequestsHistory h ON r.Id = h.Id
		WHERE
			h.CashRequestHistoryID = @CashRequestHistoryID
	END

	------------------------------------------------------------------------------
END
GO
