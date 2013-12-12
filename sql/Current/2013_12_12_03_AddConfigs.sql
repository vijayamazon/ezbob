IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EzbobMailTo')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EzbobMailTo', 'nobody@ezbob.com', 'Will define the recipient for mail designed to be sent to us')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EzbobMailCc')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EzbobMailCc', 'nobodycc@ezbob.com', 'Will define the cc recipient for mail designed to be sent to us')
END
GO
