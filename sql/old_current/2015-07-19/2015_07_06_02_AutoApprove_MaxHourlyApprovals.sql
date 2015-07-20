SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveMaxHourlyApprovals')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveMaxHourlyApprovals',
		'5',
		'Integer. Auto approval will approve until total "this setting" approvals during current calendar hour (if it is 12:14 now then calendar hour is 12:00 - 13:00).',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveMaxLastHourApprovals')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveMaxLastHourApprovals',
		'5',
		'Integer. Auto approval will approve until total "this setting" approvals during last hour (i.e. last 60 minutes).',
		0
	)
END
GO
