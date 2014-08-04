IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'DateAccessed' AND id = OBJECT_ID('BrokerLeadTokens'))
	ALTER TABLE BrokerLeadTokens ADD DateAccessed DATETIME NULL
GO
