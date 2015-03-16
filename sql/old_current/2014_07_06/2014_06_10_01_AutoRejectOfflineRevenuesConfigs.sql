IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='Reject_LowOfflineAnnualRevenue')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('Reject_LowOfflineAnnualRevenue', 30000, 'The lowest Annual revenue that offline customer needs to have in order to be rejected')
END
GO
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='Reject_LowOfflineQuarterRevenue')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('Reject_LowOfflineQuarterRevenue', 5000, 'The lowest 3 Months revenue that offline customer needs to have in order to be rejected')
END
GO
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='Reject_LateLastMonthsNum')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('Reject_LateLastMonthsNum', 3, 'The months num for checking CAIS accounts late state that customer needs to have in order to be rejected')
END
GO
IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='Reject_NumOfLateAccounts')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('Reject_NumOfLateAccounts', 2, 'The min num of CAIS months (in one or more accounts) in late state that customer needs to have in order to be rejected')
END
GO
