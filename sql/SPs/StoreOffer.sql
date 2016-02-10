IF OBJECT_ID('StoreOffer') IS NULL
	EXECUTE('CREATE PROCEDURE StoreOffer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE StoreOffer
 @CustomerId INT
,@CalculationTime DATETIME
,@Amount INT
,@MedalClassification NVARCHAR(50)	
,@ScenarioName NVARCHAR(50)
,@Period INT
,@LoanTypeId INT
,@InterestRate DECIMAL(18,6)
,@SetupFee DECIMAL(18,6)
,@Error NVARCHAR (500)
,@GradeID INT
,@SubGradeID INT
,@CashRequestID BIGINT
,@NLCashRequestID BIGINT
AS
BEGIN
	UPDATE OfferCalculations SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId

	INSERT INTO OfferCalculations (
		CustomerId, CalculationTime, IsActive, Amount, MedalClassification,
		ScenarioName, Period, LoanTypeId, InterestRate, SetupFee, Error,
		GradeID, SubGradeID, CashRequestID, NLCashRequestID
	) VALUES (
		@CustomerId, @CalculationTime, 1, @Amount, @MedalClassification,
		@ScenarioName, @Period, @LoanTypeId, @InterestRate, @SetupFee, @Error,
		@GradeID, @SubGradeID, @CashRequestID, @NLCashRequestID
	)
END
GO
