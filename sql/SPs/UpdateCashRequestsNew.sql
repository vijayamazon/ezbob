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
	@AnualTurnover INT,
	@InterestRate DECIMAL(18,4),
	@ManualSetupFeeAmount INT,
	@ManualSetupFeePercent DECIMAL(18,4),
	@RepaymentPeriod INT,
	@Now DATETIME)
AS
BEGIN
	DECLARE @MaxId INT
	SELECT @MaxId = MAX(id) FROM CashRequests WHERE IdCustomer = @CustomerId

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
			AnualTurnover = @AnualTurnover,
			InterestRate = @InterestRate,
			ManualSetupFeeAmount = @ManualSetupFeeAmount,
			ManualSetupFeePercent = @ManualSetupFeePercent
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
			AnualTurnover = @AnualTurnover,
			InterestRate = @InterestRate,
			ManualSetupFeeAmount = @ManualSetupFeeAmount,
			ManualSetupFeePercent = @ManualSetupFeePercent,
			RepaymentPeriod = @RepaymentPeriod
		WHERE 
			Id = @MaxId
	END
END
GO
