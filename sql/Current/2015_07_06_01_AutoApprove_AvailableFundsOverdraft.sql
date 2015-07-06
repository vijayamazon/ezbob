SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveAvailableFundsOverdraft')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveAvailableFundsOverdraft',
		'50000',
		'Integer. Absulute value is always used. Auto approval will approve if Available Funds - Approved Amount >= -ABS(this setting).',
		0
	)
END
GO
