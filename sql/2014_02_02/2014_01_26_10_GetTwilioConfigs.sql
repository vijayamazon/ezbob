IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SkipCodeGenerationNumber')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SkipCodeGenerationNumber', '01111111111', 'When this number is used a code won''t be generated')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SkipCodeGenerationNumberCode')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SkipCodeGenerationNumberCode', '222222', 'This is the code that will be matched against the skip number during code validation')
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTwilioConfigs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTwilioConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetTwilioConfigs]
AS
BEGIN
	SELECT
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TwilioAccountSid') AS TwilioAccountSid,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TwilioAuthToken') AS TwilioAuthToken,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TwilioSendingNumber') AS TwilioSendingNumber,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'MaxPerNumber') AS MaxPerNumber,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'MaxPerDay') AS MaxPerDay,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'SkipCodeGenerationNumber') AS SkipCodeGenerationNumber,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'SkipCodeGenerationNumberCode') AS SkipCodeGenerationNumberCode
END
GO
