IF OBJECT_ID('LoanLien') IS NULL
BEGIN
	CREATE TABLE LoanLien
	(
		Id INT IDENTITY(1,1) NOT NULL,
		Name NVARCHAR(30),
		CONSTRAINT PK_LoanLien PRIMARY KEY (Id),
		
	)
END
GO


IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LoanLienId' and Object_ID = Object_ID(N'Loan'))    
BEGIN

ALTER TABLE Loan ADD LoanLienId INT
ALTER TABLE Loan ADD CONSTRAINT FK_Loan_LoanLien FOREIGN KEY (LoanLienId) REFERENCES LoanLien(Id)
		
END 
GO