IF OBJECT_ID('CHK_LoyaltyProgramActions') IS NOT NULL
	ALTER TABLE LoyaltyProgramActions DROP CONSTRAINT CHK_LoyaltyProgramActions
GO

ALTER TABLE LoyaltyProgramActions
	ADD CONSTRAINT CHK_LoyaltyProgramActions
	CHECK (ActionID > 0 AND LTRIM(RTRIM(ActionName)) != '' AND LTRIM(RTRIM(ActionDescription)) != '')
GO

IF OBJECT_ID('CHK_CustomerLoyaltyProgram') IS NOT NULL
	ALTER TABLE CustomerLoyaltyProgram DROP CONSTRAINT CHK_CustomerLoyaltyProgram
GO

IF NOT EXISTS (SELECT * FROM LoyaltyProgramActions WHERE ActionID = 7)
	INSERT INTO LoyaltyProgramActions VALUES (7, 'PAYLOAN', 'Customer: pay loan with points', -1, 3)
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'DisplayEarnedPoints')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('DisplayEarnedPoints', '0', 'Values: 1 = yes, 0 or any other = no. Display customer earned points in sign up wizard and customer dashboard')
GO
