DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EverlineRefinanceEmailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EverlineRefinanceEmailReciever', '', 'Comma separated emails for everline refinance loan taken')
	END
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EverlineRefinanceEmailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EverlineRefinanceEmailReciever', 'qa@ezbob.com', 'Comma separated emails for everline refinance loan taken')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EverlineRefinanceEmailReciever')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EverlineRefinanceEmailReciever', '', 'Comma separated emails for everline refinance loan taken')
	END
END
GO
