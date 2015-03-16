IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='LimitedMedalMinOffer')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('LimitedMedalMinOffer', '1000', 'The minimal offer amount for limited medal')
END
GO
