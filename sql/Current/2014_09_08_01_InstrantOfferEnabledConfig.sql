IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BrokerInstantOfferEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BrokerInstantOfferEnabled', 'False', 'Use True/False to enable/disble broker instant offer tab')
END
GO
