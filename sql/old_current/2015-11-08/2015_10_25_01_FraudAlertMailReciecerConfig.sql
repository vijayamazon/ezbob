DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FraudAlertMailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FraudAlertMailReciever', '', 'email that notifies when customer adds bank account added by another customer')
	END
END

IF @Environment = 'QA'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FraudAlertMailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FraudAlertMailReciever', 'qa@ezbob.com', 'email that notifies when customer adds bank account added by another customer')
	END
END

IF @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FraudAlertMailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FraudAlertMailReciever', 'qa@ezbob.com', 'email that notifies when customer adds bank account added by another customer')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='FraudAlertMailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('FraudAlertMailReciever', 'support@ezbob.com;risk@ezbob.com;customercare@ezbob.com', 'email that notifies when customer adds bank account added by another customer')
	END
END
GO