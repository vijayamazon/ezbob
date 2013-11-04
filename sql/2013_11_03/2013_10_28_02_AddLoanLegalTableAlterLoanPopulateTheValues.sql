IF OBJECT_ID ('dbo.LoanLegal') IS NULL
BEGIN 	
	CREATE TABLE dbo.LoanLegal
		(
		  Id                                   INT IDENTITY NOT NULL
		, Created                              DATETIME DEFAULT (getutcdate()) NOT NULL
		, CashRequestsId                       BIGINT NOT NULL
		, CreditActAgreementAgreed             BIT NOT NULL DEFAULT (0)
		, PreContractAgreementAgreed           BIT NOT NULL DEFAULT (0)
		, PrivateCompanyLoanAgreementAgreed    BIT NOT NULL DEFAULT (0)
		, GuarantyAgreementAgreed              BIT NOT NULL DEFAULT (0)
		, EUAgreementAgreed                    BIT NOT NULL DEFAULT (0)
		, LoanId                               INT 
		, CONSTRAINT PK_LoanLegal              PRIMARY KEY (Id)
		, CONSTRAINT FK_LoanLegal_CashRequests FOREIGN KEY (CashRequestsId) REFERENCES dbo.CashRequests (Id)
		)
END
GO  
IF((SELECT COUNT(*) FROM LoanLegal) = 0) 
BEGIN 
INSERT INTO LoanLegal(Created,CashRequestsId,CreditActAgreementAgreed,PreContractAgreementAgreed,PrivateCompanyLoanAgreementAgreed,GuarantyAgreementAgreed, LoanId) 
(SELECT l.[Date] Created, l.RequestCashId CashRequestsId,
     CASE WHEN c.TypeOfBusiness IN ('PShip3P','Entrepreneur','SoleTrader') THEN 1 ELSE 0 END AS CreditActAgreementAgreed,
     CASE WHEN c.TypeOfBusiness IN ('PShip3P','Entrepreneur','SoleTrader') THEN 1 ELSE 0 END AS PreContractAgreementAgreed,
     CASE WHEN c.TypeOfBusiness IN ('PShip3P','Entrepreneur','SoleTrader') THEN 0 ELSE 1 END AS PrivateCompanyLoanAgreementAgreed,
     CASE WHEN c.TypeOfBusiness IN ('PShip3P','Entrepreneur','SoleTrader') THEN 0 ELSE 1 END AS GuarantyAgreementAgreed,
     l.Id 
 FROM Loan l, Customer c WHERE l.CustomerId = c.Id)
END 

GO 
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Loan') AND name = 'LoanLegalId')
BEGIN
	ALTER TABLE Loan ADD LoanLegalId INT
END
GO 
UPDATE Loan SET LoanLegalId = (SELECT Id FROM LoanLegal WHERE Loan.Id = LoanId) WHERE LoanLegalId IS NULL
GO



