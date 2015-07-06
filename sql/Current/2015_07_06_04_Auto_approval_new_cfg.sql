SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveOfficeTimeStart')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveOfficeTimeStart',
		'9:00',
		'London time. In format "hour:minute". 0 <= hour <= 23, 0 <= minute <= 59. In case of error 9:00am is used.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveOfficeTimeEnd')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveOfficeTimeEnd',
		'19:00',
		'London time. In format "hour:minute". 0 <= hour <= 23, 0 <= minute <= 59. In case of error 19:00 is used.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveWeekend')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveWeekend',
		'Sa,Su',
		'Comma separated list of not working days. Days are Su, Mo, Tu, We, Th, Fr, Sa.',
		0
	)
END
GO


IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveOffHoursMaxOutstandingOffers')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveOffHoursMaxOutstandingOffers',
		'200000',
		'Off-hours: maximal amount of outstanding offers for a customer to be auto approved.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveOffHoursMaxTodayLoans')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveOffHoursMaxTodayLoans',
		'100000',
		'Off-hours: maximal amount of todays loans for a customer to be auto approved.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveOffHoursMaxDailyApprovals')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveOffHoursMaxDailyApprovals',
		'10',
		'Off-hours: maximal number of todays auto approvals for a customer to be auto approved.',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveOffHoursMaxHourlyApprovals')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveOffHoursMaxHourlyApprovals',
		'3',
		'Off-hours: Integer. Auto approval will approve until total "this setting" approvals during current calendar hour (if it is 12:14 now then calendar hour is 12:00 - 13:00).',
		0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoApproveOffHoursMaxLastHourApprovals')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'AutoApproveOffHoursMaxLastHourApprovals',
		'3',
		'Off-hours: Integer. Auto approval will approve until total "this setting" approvals during last hour (i.e. last 60 minutes).',
		0
	)
END
GO
