IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='BrokerCommissionEnabled')
INSERT INTO dbo.ConfigurationVariables(Name, Value, Description) VALUES	('BrokerCommissionEnabled', 'False', 'Determines if the broker commission is enabled by default')
GO

