IF(object_id('OfferCalculations') IS NULL) 
BEGIN
CREATE TABLE OfferCalculations
(
	Id INT IDENTITY
	,CustomerId INT NOT NULL
	,CalculationTime DATETIME
	,IsActive BIT
	,Amount INT
	,MedalClassification NVARCHAR(50)	
	,ScenarioName NVARCHAR(50)
	,Period INT
	,IsEu BIT
	,InterestRate DECIMAL(18,6)
	,SetupFee DECIMAL(18,6)
	,Error NVARCHAR (500)
	,CONSTRAINT PK_OfferCalculations PRIMARY KEY (Id)
)
END 
GO

