IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanSource') AND name = 'DefaultRepaymentPeriod')
BEGIN
	ALTER TABLE LoanSource ADD
		DefaultRepaymentPeriod INT NULL,
		IsCustomerRepaymentPeriodSelectionAllowed BIT NOT NULL CONSTRAINT DF_LoanSource_Icrpsa DEFAULT (1)

	ALTER TABLE CashRequests ADD
		IsCustomerRepaymentPeriodSelectionAllowed BIT NOT NULL CONSTRAINT DF_CashRequests_Icrpsa DEFAULT (1)
END
GO 
UPDATE LoanSource SET DefaultRepaymentPeriod = 12, IsCustomerRepaymentPeriodSelectionAllowed = 0 WHERE LoanSourceName = 'EU'
GO
