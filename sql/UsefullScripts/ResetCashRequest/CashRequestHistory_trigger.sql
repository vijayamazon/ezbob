SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('TR_CashRequests_WriteHistory') IS NOT NULL
	DROP TRIGGER TR_CashRequests_WriteHistory
GO

CREATE TRIGGER TR_CashRequests_WriteHistory
ON CashRequests FOR INSERT, UPDATE
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
		INSERTED
END
GO
