IF OBJECT_ID('AV_StoreOffer') IS NULL
	EXECUTE('CREATE PROCEDURE AV_StoreOffer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AV_StoreOffer
	(@CustomerId INT
	,@CalculationTime DATETIME
	,@Amount INT
	,@Medal NVARCHAR(50)	
	,@ScenarioName NVARCHAR(50)
	,@Period INT
	,@IsEu BIT
	,@LoanType NVARCHAR(30)
	,@LoanSource NVARCHAR(30)
	,@InterestRate DECIMAL(18,6)
	,@SetupFee DECIMAL(18,6)
	,@Error NVARCHAR (500)
	,@Type NVARCHAR(30))
AS
BEGIN
	
	IF(@Type = 'Seek')
	BEGIN 

	UPDATE OfferCalculationsSeekAV SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId
	INSERT INTO OfferCalculationsSeekAV (CustomerId,CalculationTime,IsActive,Amount,Medal,ScenarioName,Period,IsEu,LoanType,LoanSource,InterestRate,SetupFee,Error)
	VALUES ( @CustomerId,@CalculationTime,1,@Amount,@Medal,@ScenarioName,@Period,@IsEu,@LoanType,@LoanSource,@InterestRate,@SetupFee,@Error)
	
	END 
	
	IF(@Type = 'Boundaries')
	BEGIN 

	UPDATE OfferCalculationsBoundariesAV SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId
	INSERT INTO OfferCalculationsBoundariesAV (CustomerId,CalculationTime,IsActive,Amount,Medal,ScenarioName,Period,IsEu,LoanType,LoanSource,InterestRate,SetupFee,Error)
	VALUES ( @CustomerId,@CalculationTime,1,@Amount,@Medal,@ScenarioName,@Period,@IsEu,@LoanType,@LoanSource,@InterestRate,@SetupFee,@Error)
	
	END 
	
	
END

GO