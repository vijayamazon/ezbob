IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanSource') AND name = 'MaxEmployeeCount')
BEGIN
	ALTER TABLE LoanSource ADD
		MaxEmployeeCount INT NULL,
		MaxAnnualTurnover DECIMAL(18, 2) NULL,
		IsDefault BIT NOT NULL CONSTRAINT DF_LoanSource_Default DEFAULT (0),
		AlertOnCustomerReasonType INT NULL

	ALTER TABLE CustomerReason ADD ReasonType INT NULL
END
GO

UPDATE LoanSource SET MaxEmployeeCount = 10, MaxAnnualTurnover = 1500000, AlertOnCustomerReasonType = 1 WHERE LoanSourceName = 'EU'
UPDATE LoanSource SET IsDefault = 1 WHERE LoanSourceName = 'Standard'
UPDATE CustomerReason SET ReasonType = 1 WHERE Reason = 'Other'
GO

