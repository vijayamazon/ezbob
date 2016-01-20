SET QUOTED_IDENTIFIER ON
GO

DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AccountingBrokersEmail')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'AccountingBrokersEmail', 'accountingBrokers@ezbob.com', 'Accounting Brokers Email', NULL)
	END
END

IF @Environment = 'UAT' OR @Environment = 'QA'
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AccountingBrokersEmail')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'AccountingBrokersEmail', 'qa+accbrokcc@ezbob.com', 'Accounting Brokers Email', NULL)
	END
END

IF @Environment = 'Dev' or @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AccountingBrokersEmail')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, [Description], IsEncrypted) VALUES (
			'AccountingBrokersEmail', '', 'Accounting Brokers Email', NULL)
	END
END
GO
