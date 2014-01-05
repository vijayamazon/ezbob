IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TwilioSendingNumber')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TwilioSendingNumber', '+17542276490', 'Twilio''s from number')
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
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'TwilioSendingNumber') AS TwilioSendingNumber
END
GO
