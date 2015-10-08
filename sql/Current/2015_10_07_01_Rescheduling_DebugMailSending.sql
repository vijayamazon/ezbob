SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ReschedulingDebugMail')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'ReschedulingDebugMail', '0', 'Boolean. false, to enable debug mail sending: 1(true), otherwise 0', 0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'ReschedulingDebugMailAddress')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'ReschedulingDebugMailAddress', 'elinar@ezbob.com', 'with ReschedulingDebugMail Boolean True sends to this email', 0
	)
END
GO
