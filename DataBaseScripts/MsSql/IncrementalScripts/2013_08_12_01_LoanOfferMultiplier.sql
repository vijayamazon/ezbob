IF OBJECT_ID ('dbo.LoanOfferMultiplier') IS NULL
BEGIN
	CREATE TABLE dbo.LoanOfferMultiplier
	(
		Id INT IDENTITY NOT NULL,
		StartScore INT,
		EndScore INT,
		Multiplier NUMERIC(10,2)
	)
	
	INSERT INTO LoanOfferMultiplier VALUES (0, 650, 0.65)
	INSERT INTO LoanOfferMultiplier VALUES (651, 1000000000, 1)
END	
GO
