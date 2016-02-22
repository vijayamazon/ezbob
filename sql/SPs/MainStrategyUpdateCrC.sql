SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF OBJECT_ID('MainStrategyUpdateCrC') IS NULL 
	EXECUTE('CREATE PROCEDURE MainStrategyUpdateCrC AS SELECT 1')
GO

ALTER PROCEDURE MainStrategyUpdateCrC
@CustomerID INT,
@CashRequestID BIGINT,
@OverrideApprovedRejected BIT,
@CustomerStatus NVARCHAR(250),
@Now DATETIME,
@OfferValidUntil DATETIME,
@SystemDecision NVARCHAR(50),
@MedalClassification NVARCHAR(50),
@OfferedCreditLine DECIMAL(18, 0),
@CreditResult NVARCHAR(100),
@SystemCalculatedSum DECIMAL(18, 0),
@DecidedToReject BIT,
@Reason NVARCHAR(200),
@DecidedToApprove BIT,
@IsLoanTypeSelectionAllowed BIT,
@AutoDecisionID INT,
@AutoDecisionName NVARCHAR(50),
@TotalScoreNormalized DECIMAL(8, 3),
@ExperianConsumerScore INT,
@AnnualTurnover INT,
@LoanTypeID INT,
@LoanSourceID INT,
@InterestRate DECIMAL(18, 4),
@RepaymentPeriod INT,
@ManualSetupFeePercent DECIMAL(18, 4),
@BrokerSetupFeePercent DECIMAL(18, 4),
@SpreadSetupFee BIT,
@IsCustomerRepaymentPeriodSelectionAllowed BIT,
@EmailSendingBanned BIT,
@DiscountPlanID INT,
@HasApprovalChance BIT,
@ProductSubTypeID INT
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION

	UPDATE Customer SET
		CreditResult = CASE WHEN @OverrideApprovedRejected = 1 THEN @CreditResult ELSE CreditResult END,
		Status = CASE WHEN @OverrideApprovedRejected = 1 THEN @CustomerStatus ELSE Status END,
		ApplyForLoan = CASE WHEN @DecidedToApprove = 1 THEN @Now ELSE NULL END,
		ValidFor = CASE WHEN @DecidedToApprove = 1 THEN @OfferValidUntil ELSE NULL END,
		SystemDecision = @SystemDecision,
		MedalType = @MedalClassification,
		CreditSum = @OfferedCreditLine,
		LastStatus = ISNULL(@CreditResult, 'N/A'),
		SystemCalculatedSum = @SystemCalculatedSum,
		ManagerApprovedSum = @OfferedCreditLine,
		DateRejected = CASE WHEN @DecidedToReject = 1 THEN @Now ELSE NULL END,
		RejectedReason = CASE WHEN @DecidedToReject = 1 THEN @Reason ELSE NULL END,
		NumRejects = NumRejects + CONVERT(INT, @DecidedToReject),
		DateApproved = CASE WHEN @DecidedToApprove = 1 THEN @Now ELSE NULL END,
		ApprovedReason = CASE WHEN @DecidedToApprove = 1 THEN @Reason ELSE NULL END,
		NumApproves = NumApproves + CONVERT(INT, @DecidedToApprove),
		IsLoanTypeSelectionAllowed = CASE WHEN @DecidedToApprove = 1 THEN ISNULL(@IsLoanTypeSelectionAllowed, 0) ELSE 0 END,
		LastStartedMainStrategyEndTime = @Now,
		HasApprovalChance = @HasApprovalChance
	WHERE
		Id = @CustomerID

	UPDATE CashRequests SET		
		--OfferStart = CASE WHEN @DecidedToApprove = 1 THEN @Now ELSE NULL END,
		--OfferValidUntil = CASE WHEN @DecidedToApprove = 1 THEN @OfferValidUntil ELSE NULL END,	
		OfferStart = @Now,	
		OfferValidUntil = @OfferValidUntil,		
		SystemDecision = @SystemDecision,
		SystemCalculatedSum = @SystemCalculatedSum,
		SystemDecisionDate = @Now,
		ManagerApprovedSum = @OfferedCreditLine,
		UnderwriterDecision = @CreditResult,
		UnderwriterDecisionDate = @Now,
		UnderwriterComment = @Reason,
		AutoDecisionID = @AutoDecisionID,
		MedalType = @MedalClassification,
		ScorePoints = @TotalScoreNormalized,
		ExpirianRating = @ExperianConsumerScore,
		AnualTurnover = @AnnualTurnover,
		LoanTypeID = @LoanTypeID,
		LoanSourceID = @LoanSourceID,
		InterestRate = CASE WHEN @DecidedToApprove = 1 THEN @InterestRate ELSE InterestRate END,
		ApprovedRepaymentPeriod = CASE WHEN @RepaymentPeriod != 0 THEN @RepaymentPeriod ELSE ApprovedRepaymentPeriod END,
		RepaymentPeriod = CASE WHEN @RepaymentPeriod != 0 THEN @RepaymentPeriod ELSE RepaymentPeriod END,
		ManualSetupFeePercent = @ManualSetupFeePercent,
		BrokerSetupFeePercent = @BrokerSetupFeePercent,
		IsLoanTypeSelectionAllowed = CONVERT(INT, @IsLoanTypeSelectionAllowed),
		IsCustomerRepaymentPeriodSelectionAllowed = @IsCustomerRepaymentPeriodSelectionAllowed,
		EmailSendingBanned = @EmailSendingBanned,
		DiscountPlanId = @DiscountPlanID,
		HasApprovalChance = @HasApprovalChance,
		SpreadSetupFee = @SpreadSetupFee,
		ProductSubTypeID = @ProductSubTypeID
	WHERE
		Id = @CashRequestID

	IF @AutoDecisionID IS NOT NULL
	BEGIN
		INSERT INTO DecisionHistory (
			[Date],
			Action,
			UnderwriterId,
			CustomerId,
			Comment,
			CashRequestId,
			LoanTypeId
		) VALUES (
			@Now,
			@AutoDecisionName,
			1, -- underwriter #1 is a reserved id for auto decisions
			@CustomerID,
			@Reason,
			@CashRequestID,
			@LoanTypeID
		)
	END

	COMMIT TRANSACTION
END
GO
