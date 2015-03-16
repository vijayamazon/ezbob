IF object_id('MedalCoefficients') IS NULL
BEGIN
	CREATE TABLE MedalCoefficients(
	    Id INT NOT NULL IDENTITY(1,1)
	   ,MedalFlow NVARCHAR(50) NOT NULL
	   ,Medal NVARCHAR(50) NOT NULL
	   ,AnnualTurnover DECIMAL(18, 6)
	   ,ValueAdded DECIMAL(18, 6)
	   ,FreeCashFlow DECIMAL(18, 6)
	   ,CONSTRAINT PK_MedalCoefficients PRIMARY KEY (Id)
	)
	
	INSERT INTO MedalCoefficients VALUES('Limited', 'Silver', 6, 15, 29)
	INSERT INTO MedalCoefficients VALUES('Limited', 'Gold', 8, 20, 38)
	INSERT INTO MedalCoefficients VALUES('Limited', 'Platinum', 10, 25, 48)
	INSERT INTO MedalCoefficients VALUES('Limited', 'Diamond', 12, 30, 58)
END
GO
