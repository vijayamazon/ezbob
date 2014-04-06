IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE [name] = N'Id' AND [object_id] = OBJECT_ID(N'BasicInterestRate'))
BEGIN
    DROP TABLE BasicInterestRate
	CREATE TABLE BasicInterestRate
	(
		Id INT IDENTITY,
		FromScore INT NOT NULL,
		ToScore INT NOT NULL,
		LoanInterestBase DECIMAL (18, 4) NOT NULL
	)
	
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanInterestBase) VALUES (0, 649, 0.06)
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanInterestBase) VALUES (650, 849, 0.05)
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanInterestBase) VALUES (850, 999, 0.04)
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanInterestBase) VALUES (1000, 1099, 0.03)
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanInterestBase) VALUES (1100, 1000000000, 0.02)
END
GO
