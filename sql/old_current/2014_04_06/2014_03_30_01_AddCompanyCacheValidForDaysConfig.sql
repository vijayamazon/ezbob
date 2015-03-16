IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='UpdateCompanyDataPeriodDays')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UpdateCompanyDataPeriodDays', 30, 'Number of days for which the experian company cache is valid for')
END
GO
