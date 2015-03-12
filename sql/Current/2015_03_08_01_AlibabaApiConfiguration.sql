DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment = 'QA' OR @Environment = 'UAT' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaClientEnvironment')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaClientEnvironment', 'useSandbox', 'If exists, use Alibaba Sandbox API credentials')
	END	
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaAppSecret_Sandbox')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaAppSecret_Sandbox', 'IOVVt8lbOfDE', 'Sanbox Alibaba API secret')
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaBaseUrl_Sandbox')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaBaseUrl_Sandbox', 'http://119.38.217.38:1680/openapi/', 'Sanbox Alibaba API base url')
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaUrlPath_Sandbox')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaUrlPath_Sandbox', 'param2/1/alibaba.open/partner.feedback/89978', 'Sanbox Alibaba API url path')
	END	
	
END


IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaAppSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaAppSecret', 'wpO07gmLj4xU', 'Production Alibaba API appSecret');
	END
	ELSE
	BEGIN
		UPDATE ConfigurationVariables SET  Value = 'wpO07gmLj4xU' WHERE Name = 'AlibabaAppSecret' AND [Description] = 'Production Alibaba API appSecret';
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaBaseUrl')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaBaseUrl', 'https://gw.api.alibaba.com/openapi/', 'Production Alibaba API base url')
	END
	ELSE
	BEGIN
		UPDATE ConfigurationVariables SET  Value = 'https://gw.api.alibaba.com/openapi/' WHERE Name = 'AlibabaBaseUrl' AND [Description] = 'Production Alibaba API base url';
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AlibabaUrlPath')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AlibabaUrlPath', 'param2/1/alibaba.open/partner.feedback/643480', 'Production Alibaba API url path')
	END	
	
END
GO
