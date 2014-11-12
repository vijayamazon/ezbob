IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SmallLoanScenarioLimit')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SmallLoanScenarioLimit', '15000', 'Defines the upper limit for small loan scenario for the calculator')
END
GO
