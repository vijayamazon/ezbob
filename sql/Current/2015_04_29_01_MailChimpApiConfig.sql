DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN 
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'MailChimpApiKey')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('MailChimpApiKey', '819034a89de7e96777ea86866a7bdcaf-us6', 'Mail Chimp Api Key')
	END 
END 

IF @Environment != 'Prod' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'MailChimpApiKey')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('MailChimpApiKey', 'c89b8918e6eb36c608527dfc06b7ff74-us6', 'Mail Chimp Api Key')
	END 
END 

GO