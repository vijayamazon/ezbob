IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CompanyCaisLateAlertLongMonths')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CompanyCaisLateAlertLongMonths', '12', 'Alert in UW if more than (CompanyCaisLateAlertShortPeriodThreshold) lates in this amount of last months')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CompanyCaisLateAlertShortMonths')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CompanyCaisLateAlertShortMonths', '6', 'Alert in UW if at least 1 late in this amount of last months')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CompanyCaisLateAlertShortPeriodThreshold')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CompanyCaisLateAlertShortPeriodThreshold', '3', 'Alert in UW if at least this amount of lates in (CompanyCaisLateAlertShortMonths)')
END
GO
