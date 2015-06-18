SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'SpreadSetupFeeCharge')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'SpreadSetupFeeCharge', '0', 'Denotes set-up fee spread per schedule.', 0
	)
END
GO
