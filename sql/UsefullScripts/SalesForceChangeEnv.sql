----------------Prod Env--------------------------
UPDATE dbo.ConfigurationVariables SET Value = 'Production' WHERE Name = 'SalesForceEnvironment'
UPDATE dbo.ConfigurationVariables SET Value = 'False' WHERE Name = 'SalesForceFakeMode'
UPDATE dbo.ConfigurationVariables SET Value = 'Ezca$h123' WHERE Name = 'SalesForcePassword'
UPDATE dbo.ConfigurationVariables SET Value = 'qCgy7jIz8PwQtIn3bwxuBv9h' WHERE Name = 'SalesForceToken'
UPDATE dbo.ConfigurationVariables SET Value = 'techapi@ezbob.com' WHERE Name = 'SalesForceUserName'
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerKey' -- TODO
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerSecret' -- TODO
GO

----------------Sandbox Env--------------------------
UPDATE dbo.ConfigurationVariables SET Value = 'Sandbox' WHERE Name = 'SalesForceEnvironment'
UPDATE dbo.ConfigurationVariables SET Value = 'False' WHERE Name = 'SalesForceFakeMode'
UPDATE dbo.ConfigurationVariables SET Value = 'yaron13572' WHERE Name = 'SalesForcePassword'
UPDATE dbo.ConfigurationVariables SET Value = 'RcFlUIMu3xZeBfKVvgryoOqvR' WHERE Name = 'SalesForceToken'
UPDATE dbo.ConfigurationVariables SET Value = 'techapi@ezbob.com.sandbox' WHERE Name = 'SalesForceUserName'
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerKey' -- TODO
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerSecret' -- TODO
GO

----------------DevSandbox Env--------------------------
UPDATE dbo.ConfigurationVariables SET Value = 'Sandbox' WHERE Name = 'SalesForceEnvironment'
UPDATE dbo.ConfigurationVariables SET Value = 'False' WHERE Name = 'SalesForceFakeMode'
UPDATE dbo.ConfigurationVariables SET Value = 'yaron13572' WHERE Name = 'SalesForcePassword'
UPDATE dbo.ConfigurationVariables SET Value = 'Um6lDVET6x0bRuIcA13tJqVPD' WHERE Name = 'SalesForceToken'
UPDATE dbo.ConfigurationVariables SET Value = 'techapi@ezbob.com.devsandbox' WHERE Name = 'SalesForceUserName'
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerKey' -- TODO
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerSecret' -- TODO
GO

----------------Sb1 Env--------------------------
UPDATE dbo.ConfigurationVariables SET Value = 'Sandbox' WHERE Name = 'SalesForceEnvironment'
UPDATE dbo.ConfigurationVariables SET Value = 'False' WHERE Name = 'SalesForceFakeMode'
UPDATE dbo.ConfigurationVariables SET Value = 'yaron13572' WHERE Name = 'SalesForcePassword'
UPDATE dbo.ConfigurationVariables SET Value = '5jY4oEpTcYpgjM1MpjDC5Slu1' WHERE Name = 'SalesForceToken'
UPDATE dbo.ConfigurationVariables SET Value = 'techapi@ezbob.com.sb1' WHERE Name = 'SalesForceUserName'
UPDATE dbo.ConfigurationVariables SET Value = '3MVG954MqIw6FnnPNMtQquUEWgFTeZVdS_G43_vBVQFTsidIuZJQgJ17SJv3PwyxSXgBWUjva9Zyq1pBALdmO' WHERE Name = 'SalesForceConsumerKey' 
UPDATE dbo.ConfigurationVariables SET Value = '1496232326147934946' WHERE Name = 'SalesForceConsumerSecret'
GO

----------------Fake --------------------------
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceEnvironment'
UPDATE dbo.ConfigurationVariables SET Value = 'True' WHERE Name = 'SalesForceFakeMode'
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForcePassword'
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceToken'
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceUserName'
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerKey' -- TODO
UPDATE dbo.ConfigurationVariables SET Value = '' WHERE Name = 'SalesForceConsumerSecret' -- TODOGO
GO