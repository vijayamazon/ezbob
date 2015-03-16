DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointMid')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointMid', 'secpay', 'PayPoint mid')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointVpnPassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointVpnPassword', 'secpay', 'PayPoint vpn password')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointRemotePassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointRemotePassword', 'secpay', 'PayPoint remote password')
	END	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointOptions')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointOptions', 'test_status=true', 'PayPoint options')
	END
	
	IF @Environment = 'QA' OR @Environment = 'UAT'
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointTemplateUrl')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointTemplateUrl', 'https://www.secpay.com/users/orange06/ezbob-template-test.html', 'PayPoint template url')
		END
	END
	ELSE
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointTemplateUrl')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointTemplateUrl', 'https://www.secpay.com/users/orange06/ezbob-template.html', 'PayPoint template url')
		END
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointMid')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointMid', 'orange06', 'PayPoint mid')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointVpnPassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointVpnPassword', 'ezbob2012', 'PayPoint vpn password')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointRemotePassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointRemotePassword', 'ezbob2012', 'PayPoint remote password')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointTemplateUrl')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointTemplateUrl', 'https://www.secpay.com/users/orange06/ezbob-template.html', 'PayPoint template url')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointOptions')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointOptions', 'test_status=live;repeat=true', 'PayPoint options')
	END
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointServiceUrl')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointServiceUrl', 'https://www.secpay.com/java-bin/services/SECCardService', 'PayPoint service url')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointDebugMode')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointDebugMode', 'False', 'PayPoint debug mode enabled')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointIsValidCard')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointIsValidCard', 'True', 'PayPoint is valid card')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointEnableCardLimit')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointEnableCardLimit', 'False', 'PayPoint enable card limit')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointCardLimitAmount')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointCardLimitAmount', '500', 'PayPoint card limit amount')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointEnableDebugErrorCodeN')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointEnableDebugErrorCodeN', 'True', 'PayPoint enable debug error code N')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPointValidateName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPointValidateName', 'True', 'PayPoint validate name')
END
GO


