IF NOT EXISTS(SELECT * FROM ConfigurationVariables WHERE Name = 'CustomerAnalyticsDefaultHistoryYears')
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES (
		'CustomerAnalyticsDefaultHistoryYears', '2', 'Int. Number of years to calculate NumOfLastDefaults in customer analytics.'
	)
GO
