IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('BrokerLeadAddModes') AND name = 'SendEmailOnCreate')
	ALTER TABLE BrokerLeadAddModes ADD SendEmailOnCreate INT NOT NULL CONSTRAINT DF_BrokerLeadAddModes_SendEmailOnCreate DEFAULT (0)
GO

UPDATE BrokerLeadAddModes SET
	SendEmailOnCreate = 1
WHERE
	BrokerLeadAddModeCode = 'INVITATION'
GO
