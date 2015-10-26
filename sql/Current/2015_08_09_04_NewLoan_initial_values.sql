SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

DECLARE @lastid INT

IF EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'Write Off')
	DELETE FROM LoanTransactionMethod WHERE Name = 'Write Off'

IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'WriteOff')
BEGIN
	SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'WriteOff', 0)
END

IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'ChargeBack')
BEGIN
	SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'ChargeBack', 0)
END

IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'WrongPayment')
BEGIN
	SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'WrongPayment', 0)
END

IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'SystemRepay')
BEGIN
	SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'SystemRepay', 0)
END

IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'SetupFeeOffset')
BEGIN
	SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'SetupFeeOffset', 0)
END
GO


IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'DefaultLoanCalculator')
	INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('DefaultLoanCalculator', 
	'Ezbob.Backend.CalculateLoan.LoanCalculator.LegacyLoanCalculator, Ezbob.Backend.CalculateLoan.LoanCalculator, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null', 
	'used in "new loan" calculator')

-- IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Max_Cancel_Fee_Role_Collector')
-- INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Max_Cancel_Fee_Role_Collector', 200, 'Maximal amount of late fee cancellation for user in role Collector')

-- IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Max_Cancel_Fee_Role_Underwriter')
-- INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Max_Cancel_Fee_Role_Underwriter', 1000, 'Maximal amount of late fee cancellation for user in role Underwriter')

-- IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Max_Cancel_Fee_Role_Manager')
-- INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Max_Cancel_Fee_Role_Manager', 5000, 'Maximal amount of late fee cancellation for user in role Manager')

-- IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Move_To_Next_Payment_Max_Days')
-- INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Move_To_Next_Payment_Max_Days', 15, 'Maximal days when extra principal is being forwarded to the next payment (in case of less than Collection_Move_To_Next_Payment_Max_Principal amount)')

-- IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Move_To_Next_Payment_Max_Principal')
-- INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Move_To_Next_Payment_Max_Principal', 100, 'Maximal principal amount is being forwarded to the next payment in case of less than Collection_Move_To_Next_Payment_Max_Days days late')

-- GO

