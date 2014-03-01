IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.FinishWizard') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.FinishWizard
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.FinishWizard
	(@CustomerId INT)
AS
BEGIN
	DECLARE
		@LastWizardStep INT,
		@DefaultLoanTypeId INT,
		@DefaultLoanSourceId INT,
		@DefaultRepaymentPeriod INT,
		@SetupFeeEnabled BIT,
		@DefaultDiscountPlanId INT,
		@DefaultIsCustomerRepaymentPeriodSelectionAllowed BIT,
		@BrokerCommissionEnabled BIT

	SELECT @LastWizardStep = WizardStepTypeID FROM WizardStepTypes WHERE TheLastOne = 1

	SELECT @DefaultLoanTypeId = Id FROM LoanType WHERE IsDefault = 1

	SELECT @DefaultLoanSourceId = LoanSourceID, @DefaultRepaymentPeriod = DefaultRepaymentPeriod, @DefaultIsCustomerRepaymentPeriodSelectionAllowed = IsCustomerRepaymentPeriodSelectionAllowed FROM LoanSource WHERE IsDefault = 1
	IF @DefaultRepaymentPeriod IS NULL
	BEGIN
		SELECT @DefaultRepaymentPeriod = RepaymentPeriod FROM LoanType WHERE IsDefault = 1
	END

	SELECT @SetupFeeEnabled = CASE Value WHEN 'True' THEN 1 ELSE 0 END FROM ConfigurationVariables WHERE Name='SetupFeeEnabled'

	SELECT @BrokerCommissionEnabled = CASE Value WHEN 'True' THEN 1 ELSE 0 END FROM ConfigurationVariables WHERE Name='BrokerCommissionEnabled'

	SELECT @DefaultDiscountPlanId = Id FROM DiscountPlan WHERE IsDefault = 1

	UPDATE Customer SET WizardStep = @LastWizardStep WHERE Id = @CustomerId
	INSERT INTO dbo.CashRequests
		(
		IdCustomer
		, IdUnderwriter
		, CreationDate
		, SystemDecision
		, UnderwriterDecision
		, SystemDecisionDate
		, UnderwriterDecisionDate
		, EscalatedDate
		, SystemCalculatedSum
		, ManagerApprovedSum
		, MedalType
		, EscalationReason
		, APR
		, RepaymentPeriod
		, ScorePoints
		, ExpirianRating
		, AnualTurnover
		, InterestRate
		, UseSetupFee
		, EmailSendingBanned
		, LoanTypeId
		, UnderwriterComment
		, HasLoans
		, LoanTemplate
		, IsLoanTypeSelectionAllowed
		, DiscountPlanId
		, LoanSourceID
		, OfferStart
		, OfferValidUntil
		, IsCustomerRepaymentPeriodSelectionAllowed
		, UseBrokerSetupFee
		, ManualSetupFeeAmount
		, ManualSetupFeePercent
		, QuickOfferID
		)
	VALUES
		(
		@CustomerId -- IdCustomer
		, NULL -- IdUnderwriter
		, getutcdate() -- CreationDate
		, NULL -- SystemDecision
		, NULL -- UnderwriterDecision
		, NULL -- SystemDecisionDate
		, NULL -- UnderwriterDecisionDate
		, NULL -- EscalatedDate
		, NULL -- SystemCalculatedSum
		, NULL -- ManagerApprovedSum
		, NULL -- MedalType
		, NULL -- EscalationReason
		, NULL -- APR
		, @DefaultRepaymentPeriod -- RepaymentPeriod
		, NULL -- ScorePoints
		, NULL -- ExpirianRating
		, 0 -- AnualTurnover
		, 0.06 -- InterestRate
		, @SetupFeeEnabled -- UseSetupFee
		, 0 -- EmailSendingBanned
		, @DefaultLoanTypeId -- LoanTypeId
		, NULL -- UnderwriterComment
		, 0 -- HasLoans
		, NULL -- LoanTemplate
		, 1 -- IsLoanTypeSelectionAllowed
		, @DefaultDiscountPlanId -- DiscountPlanId
		, @DefaultLoanSourceId -- LoanSourceID
		, getutcdate() -- OfferStart
		, DATEADD(day, 1, getutcdate()) -- OfferValidUntil -- qqq should be according to config 'OfferValidForHours'
		, @DefaultIsCustomerRepaymentPeriodSelectionAllowed -- IsCustomerRepaymentPeriodSelectionAllowed
		, @BrokerCommissionEnabled -- UseBrokerSetupFee
		, NULL -- ManualSetupFeeAmount
		, NULL -- ManualSetupFeePercent
		, NULL -- QuickOfferID
		)
		
	INSERT INTO ExperianConsentAgreement
		(
		Template
		, CustomerId
		, FilePath
		)
	VALUES
		(
		'',
		@CustomerId,
		''
		)
END
GO
