IF NOT EXISTS(SELECT * FROM ConfigurationVariables WHERE Name = 'BrokerSetupFeeRate')
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES (
		'BrokerSetupFeeRate',
		'0.05',
		'Either decimal number from 0 to 1 which is set up fee rate as percent of loan amount or the word "table" (case insensitive) to use BrokerSetupFee table.'
	)
GO
