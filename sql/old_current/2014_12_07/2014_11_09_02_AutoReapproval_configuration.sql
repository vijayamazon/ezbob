IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoReApproveMaxLatePayment')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description, IsEncrypted) VALUES (
		'AutoReApproveMaxLatePayment', '4', 'Integer. No auto reapproval if after the last manually approved cash request customer has a payment that is late for days than this value.', 0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoReApproveMaxLacrAge')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description, IsEncrypted) VALUES (
		'AutoReApproveMaxLacrAge', '30', 'Integer. No auto reapproval if after the last manually approved cash request happened more than this number of days ago.', 0
	)
END
GO
