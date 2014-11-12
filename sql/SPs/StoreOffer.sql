IF OBJECT_ID('StoreOffer') IS NULL
	EXECUTE('CREATE PROCEDURE StoreOffer AS SELECT 1')
GO

ALTER PROCEDURE StoreOffer
	(@CustomerId INT
	,@CalculationTime DATETIME
	,@Amount INT
	,@MedalClassification NVARCHAR(50)	
	,@ScenarioName NVARCHAR(50)
	,@Period INT
	,@IsEu BIT
	,@InterestRate DECIMAL(18,6)
	,@SetupFee DECIMAL(18,6)
	,@Error NVARCHAR (500))
AS
BEGIN
	UPDATE StoreOffer SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId

	INSERT INTO OfferCalculations (
	 CustomerId
	,CalculationTime
	,IsActive
	,Amount
	,MedalClassification
	,ScenarioName
	,Period
	,IsEu
	,InterestRate
	,SetupFee
	,Error)
	VALUES (
	 @CustomerId
	,@CalculationTime
	,1
	,@Amount
	,@MedalClassification
	,@ScenarioName
	,@Period
	,@IsEu
	,@InterestRate
	,@SetupFee
	,@Error)
END
GO
