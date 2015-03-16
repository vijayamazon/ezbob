
DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TrustPilotBccMail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TrustPilotBccMail', '', 'TrustPilotBccMail')
	END
	
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TrustPilotBccMail')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TrustPilotBccMail', 'd65d36d7@trustpilotservice.com', 'TrustPilotBccMail')
	END
END
GO