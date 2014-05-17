IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalApiAuthenticationMode')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalApiAuthenticationMode', 'ThreeToken', 'PayPal api authentication mode')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalPpApplicationId')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalPpApplicationId', 'APP-3UB991847T1143334', 'PayPal pp application id')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalApiUsername')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalApiUsername', 'caroles_api1.ezbob.com', 'PayPal api username')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalApiPassword')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalApiPassword', '8RYKUCVNC5PXWLPD', 'PayPal api password')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalApiSignature')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalApiSignature', 'AFcWxV21C7fd0v3bYYYRCpSSRl31Aoui-O9-IwfGEuUAgSnmws-LttVZ', 'PayPal api signature')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalApiRequestFormat')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalApiRequestFormat', 'SOAP11', 'PayPal api request format')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalApiResponseFormat')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalApiResponseFormat', 'SOAP11', 'PayPal api response format')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalTrustAll')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalTrustAll', 'True', 'PayPal trust all')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalServiceType')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalServiceType', 'Production', 'PayPal service type')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalNumberOfRetries')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalNumberOfRetries', '10', 'PayPal number of retries')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalMaxAllowedFailures')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalMaxAllowedFailures', '3', 'PayPal max allowed failures')
END
GO
