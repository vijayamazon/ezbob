IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='HmrcSalariesMultiplier')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('HmrcSalariesMultiplier', 0, 'Hmrc salaries multiplier for last 4 period column calculation')
END
