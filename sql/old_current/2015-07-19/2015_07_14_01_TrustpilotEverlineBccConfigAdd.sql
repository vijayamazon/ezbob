DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='TrustPilotBccMailEverline')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables	(Name, Value, Description, IsEncrypted)
		VALUES ('TrustPilotBccMailEverline', '0d79b61b88@trustpilotservice.com', 'TrustPilotBccMailEverline', NULL)
		
		UPDATE ConfigurationVariables SET Description = 'TrustPilotBccMailEzbob' WHERE Name='TrustPilotBccMail'
	END  
END

IF @Environment <> 'Prod' OR @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='TrustPilotBccMailEverline')
	BEGIN
		INSERT INTO dbo.ConfigurationVariables	(Name, Value, Description, IsEncrypted)
		VALUES ('TrustPilotBccMailEverline', '', 'TrustPilotBccMailEverline', NULL)
		
		UPDATE ConfigurationVariables SET Description = 'TrustPilotBccMailEzbob' WHERE Name='TrustPilotBccMail'
	END  
END
GO
