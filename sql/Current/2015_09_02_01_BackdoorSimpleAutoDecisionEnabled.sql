SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'BackdoorSimpleAutoDecisionEnabled')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'BackdoorSimpleAutoDecisionEnabled', '0', 'Boolean. True, to enable backdoor; false, otherwise.', 0
	)
END
GO
