DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MandrillKey')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MandrillKey', 'nNAb_KZhxEqLCyzEGOWvlg', 'Mandrill key')
	END
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MandrillKey')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MandrillKey', 'J3SGjITr1NZL-hWzUKqZJw', 'Mandrill key')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MandrillKey')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MandrillKey', 'ZpZX8rtjJMJYOCGFCA1uGg', 'Mandrill key')
	END
END
GO
