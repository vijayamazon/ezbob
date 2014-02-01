IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'TwilioSendingNumber')
BEGIN
	UPDATE ConfigurationVariables SET Value = '+441301272000' WHERE Name = 'TwilioSendingNumber'
END
ELSE
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('TwilioSendingNumber', '+441301272000', 'Twilio''s from number')
END
GO
