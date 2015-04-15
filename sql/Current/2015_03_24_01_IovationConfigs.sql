
DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberId', '883900', 'an iovation supplied value identifying your company/site')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberAccount')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberAccount', 'OLTP', 'iovation creds')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberPasscode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberPasscode', 'YMKKZR1J', 'iovation creds')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAdminAccountName')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAdminAccountName', 'orange', 'iovation creds')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAdminPassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAdminPassword', '345yP0und5', 'iovation creds')
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationEnvironment')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationEnvironment', 'Production', 'Production / Test')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationEnabled', 'True', 'True / False')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAdminUrl')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAdminUrl', 'https://admin.iovation.com/', 'iovation admin url')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationCheckPeriod')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationCheckPeriod', '30', 'iovation not performing check if last one was in the period of 30 days')
	END
	
END
ELSE	
BEGIN

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberId', '962002', 'an iovation supplied value identifying your company/site')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberAccount')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberAccount', 'OLTP', 'iovation creds')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationSubscriberPasscode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationSubscriberPasscode', '5BM9NDY2', 'iovation creds')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAdminAccountName')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAdminAccountName', 'orange', 'iovation creds')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAdminPassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAdminPassword', '345yP0und5', 'iovation creds')
	END


	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationEnvironment')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationEnvironment', 'Test', 'Production / Test')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationEnabled', 'False', 'True / False')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationAdminUrl')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationAdminUrl', 'https://ci-admin.iovation.com/', 'iovation admin url')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='IovationCheckPeriod')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('IovationCheckPeriod', '30', 'iovation not performing check if last one was in the period of 30 days')
	END
END
GO

DELETE FROM ConfigurationVariables WHERE Name IN ('IovationAccountCode', 'IovationSubscriberCode')
GO
