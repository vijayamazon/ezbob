IF OBJECT_ID('LoanTransactionMethod') IS NULL
BEGIN
	CREATE TABLE LoanTransactionMethod (
		Id INT NOT NULL,
		Name NVARCHAR(64) NOT NULL,
		DisplaySort INT NOT NULL,
		CONSTRAINT PK_LoanTransactionMethod PRIMARY KEY (Id)
	)

	INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort)
		VALUES 
			(0, 'Unknown', 0),
			(1, 'Pacnet', 0),
			(2, 'Auto', 0),
			(3, 'Manual', 0),
			(4, 'Cheque', 1),
			(5, 'Cash', 2),
			(6, 'Non-Cash', 3),
			(7, 'Bank transfer', 4),
			(8, 'Other', 5)
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanTransaction') AND name = 'LoanTransactionMethodId')
BEGIN
	ALTER TABLE LoanTransaction ADD LoanTransactionMethodId INT NOT NULL CONSTRAINT DF_LoanTransaction_MethodId DEFAULT (0)
	ALTER TABLE LoanTransaction ADD CONSTRAINT FK_LoanTransaction_MethodId FOREIGN KEY (LoanTransactionMethodId) REFERENCES LoanTransactionMethod (Id)	
END
GO
