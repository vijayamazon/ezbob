IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveExperianScoreThreshold')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveExperianScoreThreshold', 900, 'Minimal experian score for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveCustomerMinAge')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveCustomerMinAge', 22, 'Minimal age for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveCustomerMaxAge')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveCustomerMaxAge', 60, 'Maximal age for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMinTurnover1M')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMinTurnover1M', 1000, 'Minimal 1 month turnover for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMinTurnover3M')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMinTurnover3M', 1500, 'Minimal 3 months turnover for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMinTurnover1Y')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMinTurnover1Y', 7000, 'Minimal 1 year turnover for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMinMPSeniorityDays')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMinMPSeniorityDays', 365, 'Minimal seniority in days for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMaxOutstandingOffers')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMaxOutstandingOffers', 200000, 'Maximal amount of outstanding offers for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMaxTodayLoans')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMaxTodayLoans', 150000, 'Maximal amount of todays loans for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMaxDailyApprovals')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMaxDailyApprovals', 5, 'Maximal number of todays auto approvals for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMaxAllowedDaysLate')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMaxAllowedDaysLate', 7, 'Maximal number of days late for any past payment for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMaxNumOfOutstandingLoans')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMaxNumOfOutstandingLoans', 1, 'Maximal number of outstanding loans for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMinRepaidPortion')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMinRepaidPortion', 0.5, 'Minimal repaid portion of principal for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMinAmount')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMinAmount', 1000, 'Minimal approved loan amount for a customer to be auto approved')
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AutoApproveMaxAmount')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AutoApproveMaxAmount', 10000, 'Minimal approved loan amount for a customer to be auto approved')
END

GO
