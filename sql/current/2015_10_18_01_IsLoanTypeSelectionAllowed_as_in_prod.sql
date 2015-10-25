SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Environment' AND Value != 'Prod')
BEGIN
	UPDATE Customer SET IsLoanTypeSelectionAllowed = 0 WHERE IsLoanTypeSelectionAllowed IS NULL

	IF OBJECT_ID('DF_Customer_IsLoanType') IS NOT NULL
		ALTER TABLE Customer DROP CONSTRAINT DF_Customer_IsLoanType

	ALTER TABLE Customer ALTER COLUMN IsLoanTypeSelectionAllowed INT NOT NULL

	ALTER TABLE Customer ADD CONSTRAINT DF_Customer_IsLoanType DEFAULT (0) FOR IsLoanTypeSelectionAllowed
END
GO
