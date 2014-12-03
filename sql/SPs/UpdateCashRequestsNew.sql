IF OBJECT_ID('UpdateCashRequestsNew') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateCashRequestsNew AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateCashRequestsNew
@CustomerId INT,
@SystemCalculatedAmount INT,
@ManagerApprovedSum INT,
@SystemDecision VARCHAR(50),
@MedalType VARCHAR(50),
@ScorePoints NUMERIC(8,3),
@ExpirianRating INT,
@AnnualTurnover INT,
@InterestRate DECIMAL(18,4),
@ManualSetupFeeAmount INT,
@ManualSetupFeePercent DECIMAL(18,4),
@RepaymentPeriod INT,
@Now DATETIME,
@IsEu BIT,
@LoanTypeId INT,
@AutoDecisionID INT
AS
BEGIN
	DECLARE
		@MaxId INT,
		@LoanSourceId INT

	SELECT @MaxId = MAX(id) FROM CashRequests WHERE IdCustomer = @CustomerId

	SELECT @LoanSourceId = LoanSourceId FROM LoanSource WHERE LoanSourceName = CASE WHEN @IsEu = 1 THEN 'EU' ELSE 'Standard' END

	IF @LoanTypeId = 0
		SELECT @LoanTypeId = Id FROM LoanType WHERE Type = 'StandardLoanType'

	UPDATE
		CashRequests
	SET
		IdCustomer = @CustomerId,
		SystemCalculatedSum = @SystemCalculatedAmount,
		ManagerApprovedSum = @ManagerApprovedSum,
		SystemDecision = @SystemDecision,
		SystemDecisionDate= @Now, 
		MedalType= @MedalType,
		ScorePoints= @ScorePoints,
		ExpirianRating = @ExpirianRating,
		AnualTurnover = @AnnualTurnover,
		InterestRate = @InterestRate,
		ManualSetupFeeAmount = @ManualSetupFeeAmount,
		ManualSetupFeePercent = @ManualSetupFeePercent,
		LoanSourceID = @LoanSourceId,
		LoanTypeId = @LoanTypeId,
		RepaymentPeriod = CASE WHEN @RepaymentPeriod = 0 THEN RepaymentPeriod ELSE @RepaymentPeriod END,
		ApprovedRepaymentPeriod = CASE WHEN @RepaymentPeriod = 0 THEN ApprovedRepaymentPeriod ELSE @RepaymentPeriod END,
		AutoDecisionID = @AutoDecisionID
	WHERE 
		Id = @MaxId
END
GO
