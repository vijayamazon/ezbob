SET QUOTED_IDENTIFIER ON
GO

UPDATE Customer SET IsLoanTypeSelectionAllowed = 0 WHERE IsLoanTypeSelectionAllowed IS NULL
GO

IF OBJECT_ID('DF_Customer_IsLoanType') IS NOT NULL
	ALTER TABLE Customer DROP CONSTRAINT DF_Customer_IsLoanType
GO

ALTER TABLE Customer ALTER COLUMN IsLoanTypeSelectionAllowed INT NOT NULL
GO

ALTER TABLE Customer ADD CONSTRAINT DF_Customer_IsLoanType DEFAULT (0) FOR IsLoanTypeSelectionAllowed
GO
