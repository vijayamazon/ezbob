-- Iovation Prod
UPDATE ConfigurationVariables SET Value = 'https://mpsnare.iesnare.com/snare.js' WHERE Name='IovationUrl'
UPDATE ConfigurationVariables SET Value = '883900' WHERE Name='IovationSubscriberId'
UPDATE ConfigurationVariables SET Value = 'YMKKZR1J' WHERE Name='IovationSubscriberPasscode'
UPDATE ConfigurationVariables SET Value = 'https://admin.iovation.com/' WHERE Name='IovationAdminUrl'
UPDATE ConfigurationVariables SET Value = 'Production' WHERE Name='IovationEnvironment'
GO

-- Iovation Sandbox
UPDATE ConfigurationVariables SET Value = 'https://ci-mpsnare.iovation.com/snare.js' WHERE Name='IovationUrl'
UPDATE ConfigurationVariables SET Value = '962002' WHERE Name='IovationSubscriberId'
UPDATE ConfigurationVariables SET Value = '5BM9NDY2' WHERE Name='IovationSubscriberPasscode'
UPDATE ConfigurationVariables SET Value = 'https://ci-admin.iovation.com/' WHERE Name='IovationAdminUrl'
UPDATE ConfigurationVariables SET Value = 'Test' WHERE Name='IovationEnvironment'
GO