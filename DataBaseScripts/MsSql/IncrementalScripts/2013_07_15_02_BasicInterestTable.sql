CREATE TABLE dbo.BasicInterestRate
	(
	FromScore int NOT NULL,
	ToScore int NOT NULL,
	LoanIntrestBase float(53) NOT NULL
	)  ON [PRIMARY]
GO


IF NOT EXISTS (SELECT * FROM BasicInterestRate WHERE LoanIntrestBase = '0.07')
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanIntrestBase)
		VALUES ('0', '649', '0.07')
GO

IF NOT EXISTS (SELECT * FROM BasicInterestRate WHERE LoanIntrestBase = '0.06')
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanIntrestBase)
		VALUES ('650', '849', '0.06')
GO

IF NOT EXISTS (SELECT * FROM BasicInterestRate WHERE LoanIntrestBase = '0.05')
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanIntrestBase)
		VALUES ('850', '999', '0.05')
GO

IF NOT EXISTS (SELECT * FROM BasicInterestRate WHERE LoanIntrestBase = '0.04')
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanIntrestBase)
		VALUES ('1000', '1099', '0.04')
GO

IF NOT EXISTS (SELECT * FROM BasicInterestRate WHERE LoanIntrestBase = '0.03')
	INSERT INTO BasicInterestRate (FromScore, ToScore, LoanIntrestBase)
		VALUES ('1100', '9999', '0.03')
GO