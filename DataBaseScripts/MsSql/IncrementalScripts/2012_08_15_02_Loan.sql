ALTER TABLE dbo.[Loan] Drop COLUMN [InterestRate]

ALTER TABLE dbo.Loan ADD
	InterestRate decimal(18, 4) NOT NULL CONSTRAINT DF_Loan_InterestRate DEFAULT 0.06
GO