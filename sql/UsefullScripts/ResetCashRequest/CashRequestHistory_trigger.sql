SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('TR_CashRequests_WriteHistory') IS NOT NULL
	DROP TRIGGER TR_CashRequests_WriteHistory
GO

CREATE TRIGGER TR_CashRequests_WriteHistory
ON CashRequests
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CashRequestsHistory (
		Id, IdCustomer, IdUnderwriter, CreationDate, SystemDecision, UnderwriterDecision,
		SystemDecisionDate, UnderwriterDecisionDate, EscalatedDate, SystemCalculatedSum,
		ManagerApprovedSum, MedalType, EscalationReason, APR, RepaymentPeriod, ScorePoints,
		ExpirianRating, AnualTurnover, InterestRate, UseSetupFee, EmailSendingBanned,
		LoanTypeId, UnderwriterComment, HasLoans, LoanTemplate, IsLoanTypeSelectionAllowed,
		DiscountPlanId, LoanSourceID, OfferStart, OfferValidUntil,
		IsCustomerRepaymentPeriodSelectionAllowed, UseBrokerSetupFee, ManualSetupFeeAmount,
		ManualSetupFeePercent, QuickOfferID, Originator, ApprovedRepaymentPeriod,
		AutoDecisionID, BrokerSetupFeePercent, SpreadSetupFee, HasApprovalChance
	) SELECT
		Id, IdCustomer, IdUnderwriter, CreationDate, SystemDecision, UnderwriterDecision,
		SystemDecisionDate, UnderwriterDecisionDate, EscalatedDate, SystemCalculatedSum,
		ManagerApprovedSum, MedalType, EscalationReason, APR, RepaymentPeriod, ScorePoints,
		ExpirianRating, AnualTurnover, InterestRate, UseSetupFee, EmailSendingBanned,
		LoanTypeId, UnderwriterComment, HasLoans, LoanTemplate, IsLoanTypeSelectionAllowed,
		DiscountPlanId, LoanSourceID, OfferStart, OfferValidUntil,
		IsCustomerRepaymentPeriodSelectionAllowed, UseBrokerSetupFee, ManualSetupFeeAmount,
		ManualSetupFeePercent, QuickOfferID, Originator, ApprovedRepaymentPeriod,
		AutoDecisionID, BrokerSetupFeePercent, SpreadSetupFee, HasApprovalChance
	FROM
		inserted

	IF EXISTS (
		SELECT *
		FROM inserted i
		LEFT JOIN deleted d
			ON i.Id = d.Id
			AND d.UnderwriterDecision IN ('Approved', 'Rejected')
			AND ISNULL(i.UnderwriterDecision, '') != d.UnderwriterDecision
		WHERE
			d.Id IS NOT NULL
	)
	BEGIN
		ROLLBACK TRANSACTION
	END
END
GO
