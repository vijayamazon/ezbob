IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCashRequestsNew]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateCashRequestsNew]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCashRequestsNew] 
	(@CustomerId INT,
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
	@IsEu BIT)
AS
BEGIN
	DECLARE
		@MaxId INT,
		@LoanSourceId INT
		
	SELECT @MaxId = MAX(id) FROM CashRequests WHERE IdCustomer = @CustomerId

	IF @IsEu = 1
		SELECT @LoanSourceId = LoanSourceId FROM LoanSource WHERE LoanSourceName = 'EU'
	ELSE
		SELECT @LoanSourceId = LoanSourceId FROM LoanSource WHERE LoanSourceName = 'Standard'
	
	IF @RepaymentPeriod = 0
	BEGIN
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
			LoanSourceID = @LoanSourceId
		WHERE 
			Id = @MaxId
	END
	ELSE
	BEGIN
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
			RepaymentPeriod = @RepaymentPeriod,
			ApprovedRepaymentPeriod = @RepaymentPeriod,
			LoanSourceID = @LoanSourceId
		WHERE 
			Id = @MaxId
	END
END
GO
