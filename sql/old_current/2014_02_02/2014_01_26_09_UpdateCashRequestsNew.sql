IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCashRequestsNew]') AND type in (N'P', N'PC'))
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
	@ManualSetupFeePercent DECIMAL(18,4))

AS
BEGIN
DECLARE @MaxId INT
SELECT @MaxId = MAX(id) FROM CashRequests WHERE IdCustomer = @CustomerId

UPDATE
	CashRequests
SET
	IdCustomer = @CustomerId,
	SystemCalculatedSum = @SystemCalculatedAmount,
	ManagerApprovedSum = @ManagerApprovedSum,
	SystemDecision = @SystemDecision,
	SystemDecisionDate= GETUTCDATE(), 
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
GO
