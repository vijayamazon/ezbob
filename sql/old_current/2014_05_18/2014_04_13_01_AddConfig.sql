IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TargetsEnabledEntrepreneur')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TargetsEnabledEntrepreneur', 'True', 'Targets enabled entrepreneur')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GetSatisfactionEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GetSatisfactionEnabled', 'False', 'Get satisfaction enabled')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NotEnoughFundsTemplateName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NotEnoughFundsTemplateName', 'NotEnoughFunds', 'NotEnoughFunds template name')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='WizardInstructionsEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('WizardInstructionsEnabled', 'False', 'Wizard instructions enabled')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='RefreshYodleeEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('RefreshYodleeEnabled', 'True', 'Refresh yodlee enabled')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetBalanceMaxManualChange')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetBalanceMaxManualChange', '300000', 'Pacnet balance max manual change')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetBalanceWeekendLimit')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetBalanceWeekendLimit', '100000', 'Pacnet balance weekend limit')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetBalanceWeekdayLimit')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetBalanceWeekdayLimit', '50000', 'Pacnet balance weekday limit')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SkipServiceOnNewCreditLine')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SkipServiceOnNewCreditLine', 'False', 'Skip service on new credit line')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SessionTimeout')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SessionTimeout', '6000', 'Session timeout')
END
GO 

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF @Environment = 'QA' OR @Environment = 'UAT'
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NotEnoughFundsToAddress')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NotEnoughFundsToAddress', 'uatmail@ezbob.com', 'NotEnoughFundsToAddress')
		END
	END
	ELSE
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NotEnoughFundsToAddress')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NotEnoughFundsToAddress', 'dev@ezbob.com', 'NotEnoughFundsToAddress')
		END
	END
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NotEnoughFundsToAddress')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NotEnoughFundsToAddress', 'ops@ezbob.com;shirik@ezbob.com', 'NotEnoughFundsToAddress')
	END
END
GO
 